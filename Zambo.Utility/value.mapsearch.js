/*!
 * wsi.mapsearch.js
 * Scripts required for Map Search page
 *
 */

var maxResults = 30;
var defaultAddress = "PT";
var defaultZoom = 8;
var searchZoom = 13;

var mycluster = false;
var xhr = null;

function startAssetsMap(markers, clickUrl, detailsUrl){

    var infowindow = new google.maps.InfoWindow({ content: '', maxWidth: 215 });

    $("#map").gmap3({
        address: defaultAddress,
        zoom: defaultZoom,
        minZoom: 5,
        mapTypeId: google.maps.MapTypeId.HYBRID
    })
    .on('zoom_changed', function (map) {
        if (typeof mycluster === typeof undefined || mycluster === false) {
            mycluster = $('#map').gmap3().get(1).groups()[0].cluster;
        }
        if (typeof mycluster !== typeof undefined && mycluster !== false) {
            if (map.zoom >= 18) {
                mycluster.disable();
                //console.log('Map cluster OFF');
            } else {
                mycluster.enable();
                //console.log('Map cluster ON');
            }
        }
    })
    .cluster({
        size: 150,
        markers: markers,
        cb: function (markers) {
            if (markers.length > 2) {
                if (markers.length < 10) {
                    return { content: "<div class='cluster cluster-1'>" + markers.length + "</div>" }; //first cluster: 2 -> 9
                } else if (markers.length < 100) {
                    return { content: "<div class='cluster cluster-2'>" + markers.length + "</div>" }; //second cluster: 10 -> 99 
                } else {
                    return { content: "<div class='cluster cluster-3'>" + markers.length + "</div>" }; //third cluster: higher than 100
                }
            }
        }
    })
    .on('click', function (marker, clusterOverlay, cluster, event) {
        var gmap = this.get(0);
        if (typeof marker !== typeof undefined && marker !== false) {
            //marker click event
            $.ajax({
                data: { id: marker.data.id },
                datatype: "text/plain",
                type: "POST",
                url: clickUrl,
                beforeSend: function (data) {
                    infowindow.setContent($('#gmap-loading').html());
                    infowindow.open(gmap, marker);
                },
                success: function (data) {
                    var info = JSON.parse(data);
                    $('#gmap-image').attr('src', info.PhotoUrl);
                    $('#gmap-title').html(info.Title);
                    $('#gmap-price').html(info.Price);
                    $('#gmap-text').html(info.Location);
                    $('#gmap-link').attr('href', detailsUrl + '/' + marker.data.id);
                    var htmlContent = $('#gmap-infowindow').html();
                    infowindow.setContent(htmlContent);
                }
            });

        } else {
            //cluster marker click event
            gmap.panTo(clusterOverlay.markers[0].getPosition());
            gmap.setZoom(gmap.getZoom() + 1);
            //render cluster selection
            renderClusterResults(clusterOverlay, maxResults);
        }
    });

    google.maps.event.addListener(infowindow, 'content_changed', function () {
        var iwOuter = $('.gm-style-iw');
        iwOuter.parent().addClass('wsi-gm-container');
        iwOuter.addClass('wsi-gm-style-iw');
    });

    google.maps.event.addListener(infowindow, 'domready', function () {
        // Referência ao DIV que agrupa o fundo da infowindow
        var iwOuter = $('.gm-style-iw');
        // Aplicar estilos costumizados
        iwOuter.addClass('wsi-gm-style-iw');
        iwOuter.parent().addClass('wsi-gm-container');
        // Obter a div exterior
        var iwBackground = iwOuter.prev();
        // Remover o div da sombra do fundo
        iwBackground.children(':nth-child(2)').css({ 'display': 'none' });
        // Remover o div de fundo branco
        iwBackground.children(':nth-child(4)').css({ 'display': 'none' });
        // Altera a cor desejada para a sombra da cauda
        iwBackground.children(':nth-child(3)').find('div').children().css({ 'box-shadow': 'rgba(0, 0, 0, 0.6) 0px 1px 6px', 'z-index': '1' });
        // Referência ao DIV que agrupa os elementos do botão fechar
        var iwCloseBtn = iwOuter.next();
        // Aplica o efeito desejado ao botão fechar
        iwCloseBtn.css({ opacity: '1' });
        // A API aplica automaticamente 0.7 de opacidade ao botão após o evento mouseout. Esta função reverte esse evento para o valor desejado.
        iwCloseBtn.mouseout(function () { $(this).css({ opacity: '1' }); });
    });

    initializeAutocomplete();
    //google.maps.event.addListener(window, 'load', initializeAutocomplete);
    
}

function initializeAutocomplete() {
    var address = (document.getElementById('gmapsearch'));
    var autocomplete = new google.maps.places.Autocomplete(address);
    autocomplete.setTypes(['geocode']);
    google.maps.event.addListener(autocomplete, 'place_changed', function () {
        var place = autocomplete.getPlace();
        if (!place.geometry) {
            searchMap(place.name, searchZoom);
            return;
        }
        var address = '';
        if (place.address_components) {
            address = [
                (place.address_components[0] && place.address_components[0].short_name || ''),
                (place.address_components[1] && place.address_components[1].short_name || ''),
                (place.address_components[2] && place.address_components[2].short_name || '')
            ].join(' ');
            searchMap(address, searchZoom);
        }
    });
}

function searchMap(address, zoom) {
    geocoder = new google.maps.Geocoder();
    geocoder.geocode({ 'address': address }, function (results, status) {
        if (status == google.maps.GeocoderStatus.OK) {
            var map = $('#map').gmap3('get').get(0);
            map.panTo(new google.maps.LatLng(results[0].geometry.location.lat(), results[0].geometry.location.lng()));
            map.setZoom(zoom);
        } else {
            searchMap(defaultAddress, defaultZoom);
        }
    });
}

function renderClusterResults(clusterOverlay, requestLimit) {
    //check if results pane is visible (hidden for xs viewport)
    if ($('.wsi-gmap-results').is(':visible')) {
        //abort previous XMLHttpRequest
        if (xhr != null) { xhr.abort(); }
        //get cluster asset ids
        var key;
        var assets = [];
        var markers = clusterOverlay.markers;
        for (key in markers) {
            if (markers.hasOwnProperty(key)) {
                assets[key] = markers[key].data.id;
            }
        }
        if (assets.length > requestLimit) {
            //request limit is too high
            $('#gmap-results-counter').html(assets.length);
            $('#gmap-loading-results').show();
            $('#gmap-results').fadeOut(200, function () {
                $(this).html('');
                $('.wsi-gmap-no-results').show();
                $('#gmap-loading-results').hide();
            });
        } else {
            //get results list (parcial view)
            xhr = $.ajax({
                data: { ids: assets },
                datatype: "text/plain",
                type: "POST",
                url: "/Assets/GetMapResults",
                beforeSend: function (data) {
                    $('.wsi-gmap-no-results').hide();
                    $('#gmap-loading-results').show();
                    $('#gmap-results').fadeOut(200, function () {
                        $(this).html('');
                    });
                },
                success: function (data) {
                    xhr = null;
                    $('#gmap-results').stop().html(data).fadeIn(400, function () {
                        $('#gmap-loading-results').hide();
                    });
                }
            });
        }
    } else {
        //clear results pane
        $('#gmap-results').html('');
        $('.wsi-gmap-no-results,#gmap-loading-results').hide();
    }
}
