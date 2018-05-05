//
//  IMensagemBll.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 07/07/2016 21:56
//
//  Copyright (c) 2016 Zambo Tecnologia Ltda
//

namespace Zambo.Utility.Interface
{
	public interface IMensagemBll
	{
		void ConfigurarServidor(string servidor, string usuario, string senha, int porta);
		Models.GeneralResponseModel<bool> EnviarMensagem(Models.MensagemEmailModel mensagem);
		Models.MensagemEmailModel MontarCabecalhoCorpoEmail(object dadosParaPreencherOCorpo);
	}
}

