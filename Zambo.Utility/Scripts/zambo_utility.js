/*!
 * zambo_utility.js v1.0.29072016
 * Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
 * Created: 15/07/2016 11:55
 * Copyright (c) 2016 Zambo Tecnologia Ltda
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

// $(document).ready do jQuery
document.onreadystatechange = function () {
    if (document.readyState == "complete") {
        focarNoPrimeiroCampo();
        bindMascaras();
    }

    //Bloqueia o clique direito
    $(this).bind("contextmenu", function (e) {
        e.preventDefault();
    });

    /////Bloqueia o F12
    //document.onkeydown = function (event) {
    //    event = (event || window.event);
    //    if (event.keyCode == 123 || event.keyCode == 18) {
    //        event.preventDefault();
    //        return false;
    //    }
    //}

    /*
    * Configuração do Datepicker for Bootstrap v1.6.0 (https://github.com/eternicode/bootstrap-datepicker)
    * Exemplo de uso:
    * <!-- DATA DE NASCIMENTO -->
    * <div id="idFormGroupDataNascimento" class="col-md-2 form-group validar-campo">
	* 	<label class="control-label" for="DataNascimento">NASCIMENTO<span class="obrigatorio">*</span></label>
    *     <div class="input-group date date_nasc" data-provide="datepicker" data-date-language="pt-BR" data-date-end-date="0d">
    *         <input id="DataNascimento" class="form-control nascimento" type="text" name="DataNascimento" maxlength="10">
    *         <div class="input-group-addon">
    *             <span class="glyphicon glyphicon-th"></span>
    *         </div>
    *     </div>
    * </div>
    */
    $.fn.datepicker.dates['pt-BR'] = {
        days: ["Domingo", "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado"],
        daysShort: ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"],
        daysMin: ["Do", "Se", "Te", "Qu", "Qu", "Se", "Sa"],
        months: ["Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"],
        monthsShort: ["Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez"],
        today: "Hoje",
        monthsTitle: "Meses",
        clear: "Limpar",
        format: "dd/mm/yyyy"
    };
    $('.date_nasc').datepicker({
        todayHighlight: true,
        clearBtn: true,
        autoclose: true
    });
}

function focarNoPrimeiroCampo() {
    $('input[type=text], textarea, input[type=radio], select').filter(':not([readonly])').filter(':not([disabled])').first().focus();
}

/*
* Coloca mascaras nos campos.
* Necessita da biblioteca jquery.mask.js de Igor Escobar
* https://igorescobar.github.io/jQuery-Mask-Plugin/
*/
function bindMascaras() {
    $('.cep').mask('99999-999');
    $('.cpf').mask('999.999.999-99');
    $('.nascimento').mask('99/99/9999');
    $('.telefone').mask('(99) 9 9999-9999');
    $('.mes').mask('99');
    $('.codigo-seguranca').mask('999');
    $('.ano').mask('9999');
    $('.numero-casa').mask('99999');
    $('.numero-cartao').mask('9999999999999999');
    $('.money').mask("###.##0,00", {reverse: true});
    $('.uf').mask('ZZ', { translation: { 'Z': { pattern: /[a-zA-Z]/, optional: true } } });

    $('.cep, .cpf, .nascimento, .telefone, .uf, .money').on('drop', function (event) {
        event.preventDefault();
        return true;
    });
}

function isFormValido() {
	var pendencias = [];

    $('.validar-campo').each(function (index, value) {
        var campo = $(this).find('select, textarea, input, radio, checkbox');
        if (campo != null) {
            if (campo.val().trim() === '') {
                var elementoLabel = $(this).find('label');
                if (elementoLabel !== null) {
                    var label = elementoLabel.text();
                        pendencias.push(label.substring(0, label.lastIndexOf('*')));
						$(value).addClass('has-error');
                }                
            } else {
                $(value).removeClass('has-error');
            }
        }
    });

    if (pendencias.length > 0) {
        return false;
    } else {
        return true;   
    }
    /*
    * Exemplo de uso
    * 
	*    <!-- NOME COMPLETO-->
	*    <div id="idFormGroupNomeCompleto" class="col-md-4 form-group validar-campo">
	*		<label class="control-label" for="NomeCompleto">NOME COMPLETO<span class="obrigatorio">*</span></label>
	*       <input class="form-control" type="text" name="NomeCompleto" id="NomeCompleto" maxlength="60">
	*    </div>
    */
}

/*
* http://www.devmedia.com.br/validando-e-mail-em-inputs-html-com-javascript/26427
* Uso: <input type="text" name="email" onblur="validacaoEmail(f1.email)" maxlength="60" size='65'>
*/
function isEmailValido(campo) {
	usuario = campo.substring(0, campo.indexOf("@"));
	dominio = campo.substring(campo.indexOf("@")+ 1, campo.length);

	if ((usuario.length >=1) &&
	    (dominio.length >=3) && 
	    (usuario.search("@")==-1) && 
	    (dominio.search("@")==-1) &&
	    (usuario.search(" ")==-1) && 
	    (dominio.search(" ")==-1) &&
	    (dominio.search(".")!=-1) &&      
	    (dominio.indexOf(".") >=1)&& 
	    (dominio.lastIndexOf(".") < dominio.length - 1)) {
		return true;
	} else {
		return false;
	}
}

function mostrarLoading() {
    $('body').append('<div id="overlay-carregando" style="background: #ffffff repeat-x; opacity: .75; filter: Alpha(Opacity=75); position: fixed; top: 0; left: 0; width: 100%; height: 100%; z-index: 100;">'+
    					'<div style="background: url(../Content/images/loader.gif) no-repeat 16px bottom; color: #069; padding: 16px 16px 16px 48px; font-size: 25px; height: 45px; width: 180px; position: absolute; top: 45%; left: 43%;">Carregando...</div>'+
    				'</div>');
}

function removerLoading() {
    $("#overlay-carregando").remove();
}

criarDialog = function (idDialog, title, message) {
  var html = '<div class="modal fade" id="' + idDialog + '" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">' +
              '<div class="modal-dialog" role="document">' +
                '<div class="modal-content">' +
                  '<div class="modal-header">' +
                    '<button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>' +
                    '<h4 class="modal-title">' + title + '</h4>' +
                  '</div>' +
                  '<div class="modal-body">' +
                  		message +
                      '<br>' +
                      '<div class="row">' +
                        '<div class="col-md-6">' +
                            '<strong class="font_strong">Atendimento ao Cliente: 11 2107-0300</strong>' +
                        '</div>' +
                        '<div class="col-md-6">' +
                            '<strong class="font_strong">Central de Assistência: 0800 979 9787</strong>' +
                        '</div>' +
                      '</div>' +
                  '</div>' +
                  '<div class="modal-footer">' +
                    '<button type="button" class="btn btn-default" data-dismiss="modal">Close</button>' +
                  '</div>' +
                '</div>' +
              '</div>' +
            '</div>';
  
  $('#' + idDialog).remove();
  $('body').append(html);
  
  $('#' + idDialog).modal({
    keyboard: true
  });
}

/*
* @function
* @name montarHtmlApartirDoArray
* @description REcebe um array contendo trechos de html em cada registro e monta e retorna uma string html
*/
montarHtmlApartirDoArray = function (template) {
	var html = '';

	for (var i = 0; i < template.length; i++) {
	  html += template[i];
	}

	return html;
}

limparInnerHTML = function (idDiv) {
	var conteudo = document.getElementById(idDiv),
		retorno = false;
	if (conteudo.innerHTML.length > 0) {
		document.getElementById(idDiv).innerHTML = '';
		retorno = true;
	}

	return retorno;
}

function replaceme(str, regex, nv) {
  return str.replace(regex, nv);
}

function validarCPF(cpf) {

	cpf = cpf.replace(/[^\d]+/g,'');

	if (cpf === '') {
		return false;
	}

	// Elimina CPFs invalidos conhecidos
	if (cpf.length != 11 ||
		cpf == "00000000000" ||
		cpf == "11111111111" ||
		cpf == "22222222222" ||
		cpf == "33333333333" ||
		cpf == "44444444444" ||
		cpf == "55555555555" ||
		cpf == "66666666666" ||
		cpf == "77777777777" ||
		cpf == "88888888888" ||
		cpf == "99999999999" ||
		cpf == "12345678910") {
		return false;
	}
	
	// Valida 1o digito
	var add = 0;
	for (var i = 0; i < 9; i++) {
		add += parseInt(cpf.charAt(i)) * (10 - i);
	}

	var rev = 11 - (add % 11);
	if (rev == 10 || rev == 11) {
		rev = 0;
	}
	if (rev != parseInt(cpf.charAt(9))) {
		return false;
	}
	
	// Valida 2o digito
	add = 0;
	for (i = 0; i < 10; i ++) {
		add += parseInt(cpf.charAt(i)) * (11 - i);
	}

	rev = 11 - (add % 11);
	if (rev == 10 || rev == 11) {
		rev = 0;
	}
	if (rev != parseInt(cpf.charAt(10))) {
		return false;
	}

	return true;  
}

if (typeof String.prototype.trim !== 'function') {
    String.prototype.trim = function () {
        return this.replace(/^\s+|\s+$/g, '');
    }
}

function ConverterStringToDate(data) {
    var pattern = /(\d{2})\/(\d{2})\/(\d{4})/;
    return new Date(data.replace(pattern, '$3/$2/$1'));
}

function converterDataServidor(data) {
    if (data) {
        var datafinal = moment(new Date(parseInt(data.substr(6))));
        return datafinal.format('DD/MM/YYYY');
    } else {
        return "";
    }
}

function calculaIdade(textoNascimento) {
    if (textoNascimento === null ||
      textoNascimento == undefined ||
      textoNascimento == '') {
        return '';
    }
    else {

        var dataFormatada = formataDataFromDatepicker(textoNascimento);
        dob = new Date(dataFormatada);
        var today = new Date();
        var age = Math.floor((today - dob) / (365.25 * 24 * 60 * 60 * 1000));
        if (isNaN(age))
            age = 0;
        return age;
    }
}

function formataDataFromDatepicker(data) {
    var dia = data.substring(0, 2);
    var mes = data.substring(3, 5);
    var ano = data.substring(6, 10);

    return ano + '-' + mes + '-' + dia;
}

function CriaArray(n) {
    this.length = n;
}

function GetDiaSemana(data) {
    NomeDia = new CriaArray(7);
    NomeDia[0] = "Segunda-feira";
    NomeDia[1] = "Ter&ccedil;a-feira";
    NomeDia[2] = "Quarta-feira";
    NomeDia[3] = "Quinta-feira";
    NomeDia[4] = "Sexta-feira";
    NomeDia[5] = "S&aacute;bado";
    NomeDia[6] = "Domingo";

    data = formataDataFromDatepicker(data);
    var dia = new Date(data);
    return HtmlDecode(NomeDia[dia.getDay()]);
}


/*
* Função para evitar digitação de caracteres em campos Numéricos
* Obtém cada tecla digitada nos campos que possuem a classe "Numerico" para verificação.
*/
function bindNumerico() {
    $('.numerico').keypress(function (evt) {
        var charCode = (evt.which) ? evt.which : event.keyCode;
        if (charCode > 31 && (charCode < 48 || charCode > 57)) {
            return false;
        }

        return true;
    });
}

function bindNumericKeypress() {
    ///classe que deixa os textBox aceitarem somente números
    $(".numeric").keydown(function (e) {
        // Allow: backspace, delete, tab, escape, enter 
        if ($.inArray(e.keyCode, [46, 8, 9, 27, 13]) !== -1 ||
            // Allow: Ctrl+A
            (e.keyCode == 65 && e.ctrlKey === true) ||
            // Allow: home, end, left, right
            (e.keyCode >= 35 && e.keyCode <= 39)) {
            // let it happen, don't do anything
            return;
        }
        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
            e.preventDefault();
        }
    });
}

function bindDecimalKeypress() {
    $(".decimal").keydown(function (e) {

        // Ensure only one comma
        if (($(this).val().match(/,/g) || []).length == 1 && (e.keyCode == 188 || e.keyCode == 110)) {
            e.preventDefault();
            return;
        }

        // Allow: backspace, delete, tab, escape, enter 
        if ($.inArray(e.keyCode, [46, 8, 9, 27, 13]) !== -1 ||
            // Allow: Ctrl+A
            (e.keyCode == 65 && e.ctrlKey === true) ||
            // Allow: ,
            (e.keyCode == 188 || e.keyCode == 110) ||
            // Allow: home, end, left, right
            (e.keyCode >= 35 && e.keyCode <= 39)) {
            // let it happen, don't do anything
            return;
        }

        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
            e.preventDefault();
        }
    });
}

function transformarArrayString(data) {
    var retorno = "";

    if (data == undefined) return retorno;

    for (var i = 0; i < data.length; i++) {
        if (retorno == "") {
            retorno = data[i];
        }
        else {
            retorno += '<br />' + data[i];
        }
    }
    if (!retorno) {
        retorno = '';
    }
    return retorno;
}

function autocomplete(idLista, idCampo) {
    var lista = 'ul' + idLista + ' li';
    var listaItens = [];

    $(lista).each(function () {
        listaItens.push($(this).text().trim());
    });

    $(idCampo).autocomplete({
        minLength: 3,
        source: listaItens,
        delay: 200
    });
}

function criarEventosDePopup() {
    var msgErro = $("#hdnMsgErro").val();
    if (msgErro != null && msgErro != "") {
        mostrarErroPopup(msgErro);
        $("#hdnMsgErro").val("");
    }

    var msgSucesso = $("#hdnMsgSucesso").val();
    if (msgSucesso != null && msgSucesso != "") {
        mostrarSucessoPopup(msgSucesso);
        $("#hdnMsgSucesso").val("");
    }
}

/* As funções de popup abaixo necessitam do jquery-ui*/
function mostrarSucessoPopup(msg) {
    if (msg != null && msg != "") {
        $(".dialog-msg-sucesso").remove();

        var html = "<div id='dialog-msg-sucesso' class='dialog-msg-sucesso msgSucesso'>" +
                        "<div id='msg-sucesso'>" +
                            "<span class='control-label'>" + msg + "</span>" +
                        "</div>" +
                    "</div>";

        $("#renderbody").append(html);

        return $(".dialog-msg-sucesso").dialog({
            autoOpen: true,
            width: 466,
            resizable: false,
            modal: true,
            title: 'Sucesso!',
            dialogClass: 'dialogSucesso',
            beforeClose: function (event, ui) {
                $(".overlay-carregando").remove();
            }
        });
    }
}

function mostrarInfoPopup(msg) {
    if (msg != null && msg != "") {
        $(".dialog-msg-info").remove();

        var html = "<div id='dialog-msg-info' class='dialog-msg-info msgInfo'>" +
                        "<div id='msg-info'>" +
                            "<span class='control-label'>" + msg + "</span>" +
                        "</div>" +
                    "</div>";

        $("#renderbody").append(html);

        return $(".dialog-msg-info").dialog({
            autoOpen: true,
            width: 466,
            resizable: false,
            modal: true,
            title: 'Informativo',
            dialogClass: 'dialogInfo',
            beforeClose: function (event, ui) {
                $(".overlay-carregando").remove();
            },
            buttons: {
                OK: function () {
                    $(this).dialog("close");
                }
            }
        });
    }
}

function mostrarErroPopup(msg) {
    if (msg != null && msg != "") {
        $(".dialog-msg-erro").remove();

        var html = "<div id='dialog-msg-erro' class='dialog-msg-erro msgErro'>" +
                        "<div id='msg-erro'>" +
                            "<span class='control-label'>" + msg + "</span>" +
                        "</div>" +
                    "</div>";

        $("#renderbody").append(html);

        return $(".dialog-msg-erro").dialog({
            autoOpen: true,
            width: 466,
            resizable: false,
            modal: true,
            title: 'Erro!',
            dialogClass: 'dialogErro',
            beforeClose: function (event, ui) {
                $(".overlay-carregando").remove();
            }
        });
    }
}

/*
* Cria uma caixa de dialógo com o botões Sim e Não com propagação do evento click quando pressiona Sim.
* Para ser utilizada com o botão submit do form.
*/
function criarDialog(idDialog, idConfirmar, idCancelar, idClick, msg) {
    var html = '<div id="' + idDialog + '">' +
                    '<p>' + msg + '</p>' +
                    '<div class="div-botao">' +
                        '<button class="btn btn-primary" id="' + idCancelar + '">' +
                            'N&atilde;o' +
                        '</button>' +
                        '<button class="btn btn-default" id="' + idConfirmar + '">' +
                            'Sim' +
                        '</button>' +
                    '</div>' +
                '</div>';

    $('#' + idDialog).remove();

    $("body").append(html);

    $('#' + idDialog).dialog({
        autoOpen: false,
        width: 466,
        modal: true,
        title: 'Confirma',
        resizable: false
    });

    $('#' + idConfirmar).click(function () {
        $('#' + idDialog).dialog('close');
        $('#' + idClick).unbind('click');
        $('#' + idClick).click();
    });

    $('#' + idCancelar).click(function () {
        $('#' + idDialog).dialog('close');
    });
}

/*
* Cria uma caixa de dialógo com os botões Sim e Não e chama uma função quando termina a execução
*/
function criarDialogComCallback(idDialog, idConfirmar, idCancelar, callbackFunction, msg) {
    var html = '<div id="' + idDialog + '">' +
                    '<p>' + msg + '</p>' +
                    '<div class="div-botao">' +
                        '<button class="btn btn-primary" id="' + idCancelar + '">' +
                            'N&atilde;o' +
                        '</button>' +
                        '<button class="btn btn-default" id="' + idConfirmar + '">' +
                            'Sim' +
                        '</button>' +
                    '</div>' +
                '</div>';

    $('#' + idDialog).remove();

    $("body").append(html);

    $('#' + idDialog).dialog({
        autoOpen: false,
        width: 466,
        modal: true,
        title: 'Confirma',
        resizable: false
    });

    $('#' + idConfirmar).click(function () {
        $('#' + idDialog).dialog('close');
        callbackFunction();
    });

    $('#' + idCancelar).click(function () {
        $('#' + idDialog).dialog('close');
    });
}

/*
* Cria duas divs e adiciona este html em uma div do Layout Padrão
* Parametros: idDivPopup - nome do id da div do Popup que será criada entre aspas simples ' sem o sustenido '#'
*               idDivForm - nome do id da div do Formulário que será criada entre aspas simples ' sem o sustenido '#'
*               titulo - Titulo do formulário
*/
function adicionarHtmlPopup(idDivPopup, idDivForm, titulo) {
    var popup = '#' + idDivPopup;
    var form = '#' + idDivForm;

    var html = "<div id=" + "'" + idDivPopup + "'" + ">" +
                    "<div id=" + "'" + idDivForm + "'" + " title=" + "'" + titulo + "'" + " style='display:none;'></div>" +
                "</div>";

    $(popup).remove();

    $("#renderbody").append(html);

    bindEventos(popup, form);
}

/*
* Abre o dialógo do Jquery UI
* Parametros: idDivPopup - nome do id da div do Popup que será criada entre aspas duplas ' com o sustenido '#'
*               idDivForm - nome do id da div do Formulário que será criada entre aspas duplas ' com o sustenido '#'
*/
function bindEventos(idDivPopup, idDivForm) {
    $(idDivForm).dialog({
        autoOpen: false,
        width: 960,
        modal: true,

        open: function () {
            $(".ui-widget-overlay").appendTo(idDivPopup);
            $(this).dialog("widget").appendTo(idDivPopup);
        },
        close: function () {
            $(".ui-widget-overlay").remove();
            $(idDivForm).dialog("close");
        }
    });
}

function htmlEncode(valor) {
    return $('<div/>').text(valor).html();
}

function post_to_url(path, params, method) {
    method = method || "post"; // Set method to post by default, if not specified.

    var form = $(document.createElement("form"))
        .attr({
            "method": method, "action": path
        });

    $.each(params, function (key, value) {
        $.each(value instanceof Array ? value : [value], function (i, val) {
            $(document.createElement("input"))
                .attr({
                    "type": "hidden", "name": key, "value": val
                })
                .appendTo(form);
        });
    });

    form.appendTo(document.body).submit();
}

function getQueryString(variavel) {
    var variaveis = location.search.replace(/\x3F/, "").replace(/\x2B/g, " ").split("&")
    var nvar
    if (variaveis != "") {
        var qs = []
        for (var i = 0; i < variaveis.length; i++) {
            nvar = variaveis[i].split("=")
            qs[nvar[0]] = unescape(nvar[1])
        }
        return qs[variavel]
    }
    return null
}


function isDataValida(data) {
    return moment(data, ["DD-MM-YYYY"]).isValid()
}

function limparCampos() {
	$("input[type=text], input[type=email], textarea").val("");
}

function limparTodosCampos(elemento) {
    $(elemento).find(':input').each(function () {
        switch (this.type) {
            case 'email':
            case 'password':
            case 'select-multiple':
            case 'select-one':
            case 'text':
            case 'textarea':
                $(this).val('');
                break;
            case 'checkbox':
            case 'radio':
                this.checked = false;
                break;
        }
    });
}

/*
* Função para desabilitar o uso da combinação de tecla Ctrl+V em qualquer campo 
* que utilizar a classe no-copy-paste.
* Pode ser utilizada em conjunto com a classe numerico.
* https://stackoverflow.com/questions/2903991/how-to-detect-ctrlv-ctrlc-using-javascript
*/
function bindNoCopyPaste() {
    var ctrlDown = false;
    var ctrlKey = 17, vKey = 86, cKey = 67;

    $(document).keydown(function (e) {
        if (e.keyCode == ctrlKey) ctrlDown = true;
    }).keyup(function (e) {
        if (e.keyCode == ctrlKey) ctrlDown = false;
    });

    $(".no-copy-paste").keydown(function (e) {
        if (ctrlDown && (e.keyCode == vKey || e.keyCode == cKey)) return false;
    });
}


/*
* CSS:
* ul.lista-erros,
*	.validation-summary-errors ul {
*	  list-style-type: none;
*	  padding: 0px;
*	}
*
*	ul.lista-erros li:before,
*	.validation-summary-errors li:before {
*	    content: "- ";
*	}
*/
function adicionaPendencias(pendencias) {
    var listaPendencias;

    listaPendencias = "<ul class='lista-erros'>";
    pendencias.forEach(function (value) {
        msg = value;
        listaPendencias += "<li>" + msg + "</li>";
    });
    listaPendencias += "</ul>";

    return listaPendencias;
}

/*
 * Extraído de https://gist.github.com/ricardodantas/6031749
 */
// JavaScript Document
//adiciona mascara de cnpj
function MascaraCNPJ(cnpj) {
    if (mascaraInteiro(cnpj) == false) {
        event.returnValue = false;
    }
    return formataCampo(cnpj, '00.000.000/0000-00', event);
}

//adiciona mascara de cep
function MascaraCep(cep) {
    if (mascaraInteiro(cep) == false) {
        event.returnValue = false;
    }
    return formataCampo(cep, '00.000-000', event);
}

//adiciona mascara de data
function MascaraData(data) {
    if (mascaraInteiro(data) == false) {
        event.returnValue = false;
    }
    return formataCampo(data, '00/00/0000', event);
}

//adiciona mascara ao telefone
function MascaraTelefone(tel) {
    if (mascaraInteiro(tel) == false) {
        event.returnValue = false;
    }
    return formataCampo(tel, '(00) 0000-0000', event);
}

//adiciona mascara ao CPF
function MascaraCPF(cpf) {
    if (mascaraInteiro(cpf) == false) {
        event.returnValue = false;
    }
    return formataCampo(cpf, '000.000.000-00', event);
}

//valida telefone
function ValidaTelefone(tel) {
    exp = /\(\d{2}\)\ \d{4}\-\d{4}/
    if (!exp.test(tel.value))
        alert('Numero de Telefone Invalido!');
}

//valida CEP
function ValidaCep(cep) {
    exp = /\d{2}\.\d{3}\-\d{3}/
    if (!exp.test(cep.value))
        alert('Numero de Cep Invalido!');
}

//valida data
function ValidaData(data) {
    exp = /\d{2}\/\d{2}\/\d{4}/
    if (!exp.test(data.value))
        alert('Data Invalida!');
}

//valida o CPF digitado
function ValidarCPF(Objcpf) {
    var cpf = Objcpf.value;
    exp = /\.|\-/g
    cpf = cpf.toString().replace(exp, "");
    var digitoDigitado = eval(cpf.charAt(9) + cpf.charAt(10));
    var soma1 = 0, soma2 = 0;
    var vlr = 11;

    for (i = 0; i < 9; i++) {
        soma1 += eval(cpf.charAt(i) * (vlr - 1));
        soma2 += eval(cpf.charAt(i) * vlr);
        vlr--;
    }
    soma1 = (((soma1 * 10) % 11) == 10 ? 0 : ((soma1 * 10) % 11));
    soma2 = (((soma2 + (2 * soma1)) * 10) % 11);

    var digitoGerado = (soma1 * 10) + soma2;
    if (digitoGerado != digitoDigitado)
        alert('CPF Invalido!');
}

//valida numero inteiro com mascara
function mascaraInteiro() {
    if (event.keyCode < 48 || event.keyCode > 57) {
        event.returnValue = false;
        return false;
    }
    return true;
}

//valida o CNPJ digitado
function ValidarCNPJ(ObjCnpj) {
    var cnpj = ObjCnpj.value;
    var valida = new Array(6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2);
    var dig1 = new Number;
    var dig2 = new Number;

    exp = /\.|\-|\//g
    cnpj = cnpj.toString().replace(exp, "");
    var digito = new Number(eval(cnpj.charAt(12) + cnpj.charAt(13)));

    for (i = 0; i < valida.length; i++) {
        dig1 += (i > 0 ? (cnpj.charAt(i - 1) * valida[i]) : 0);
        dig2 += cnpj.charAt(i) * valida[i];
    }
    dig1 = (((dig1 % 11) < 2) ? 0 : (11 - (dig1 % 11)));
    dig2 = (((dig2 % 11) < 2) ? 0 : (11 - (dig2 % 11)));

    if (((dig1 * 10) + dig2) != digito)
        alert('CNPJ Invalido!');

}

//formata de forma generica os campos
function formataCampo(campo, Mascara, evento) {
    var boleanoMascara;

    var Digitato = evento.keyCode;
    exp = /\-|\.|\/|\(|\)| /g
    campoSoNumeros = campo.value.toString().replace(exp, "");

    var posicaoCampo = 0;
    var NovoValorCampo = "";
    var TamanhoMascara = campoSoNumeros.length;;

    if (Digitato != 8) { // backspace 
        for (i = 0; i <= TamanhoMascara; i++) {
            boleanoMascara = ((Mascara.charAt(i) == "-") || (Mascara.charAt(i) == ".")
                                                    || (Mascara.charAt(i) == "/"))
            boleanoMascara = boleanoMascara || ((Mascara.charAt(i) == "(")
                                                    || (Mascara.charAt(i) == ")") || (Mascara.charAt(i) == " "))
            if (boleanoMascara) {
                NovoValorCampo += Mascara.charAt(i);
                TamanhoMascara++;
            } else {
                NovoValorCampo += campoSoNumeros.charAt(posicaoCampo);
                posicaoCampo++;
            }
        }
        campo.value = NovoValorCampo;
        return true;
    } else {
        return true;
    }
}
/*
 * Extraído de https://gist.github.com/ricardodantas/6031749
 */