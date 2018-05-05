//
//  MensagemBll.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 24/05/2016 16:17
//
//  Copyright (c) 2016 Zambo Tecnologia Ltda
//

using System;
using System.Net.Mail;
using System.Text;

namespace Zambo.Utility
{
	public class MensagemBll : Interface.IMensagemBll
	{
		/// <summary>
		/// Endereço do servidor. Ex: sheol.travelace.com.br
		/// </summary>
		string host;

		/// <summary>
		/// Endereço de email de uma conta válida. Ex: cjesus@travelace.com.br
		/// </summary>
		string userName;

		/// <summary>
		/// Senha do email
		/// </summary>
		string pass;

		int porta;

		public MensagemBll()
		{
			// Abra a configuração de um arquivo Web.Config de um projeto ASP.NET MVC
			// Exemplo de Web.Config
			/*
			   <appSettings>
				 <add key="mailServer" value="smtp.zambotecnologia.com.br,587,noreply,123456" />   
			   </appSettings>
			*/
			var mailServer = System.Configuration.ConfigurationManager.AppSettings["mailServer"].Split(',');
			this.host = mailServer[0];
			this.porta = Convert.ToInt32(mailServer[1]);
			this.userName = mailServer[2];
			this.pass = mailServer[3];
		}

		/// <summary>
		/// Caso deseje utilizar um servidor de email diferente ou se autenticar utilizando outro endereço de email
		/// </summary>
		/// <param name="servidor">Endereço do servidor. Ex: sheol.zambotecnologia.com.br</param>
		/// <param name="usuario">Endereço de email de uma conta válida. Ex: cjesus@zambotecnologia.com.br</param>
		/// <param name="senha">Senha do email</param>
		public void ConfigurarServidor(string servidor, string usuario, string senha, int porta)
		{
			this.host = servidor;
			this.userName = usuario;
			this.pass = senha;
			this.porta = porta;
		}

		/// <summary>
		/// Envia uma mensagem qualquer por email
		/// </summary>
		/// <param name="mensagem">Models.MensagemEmailModel</param>
		/// <returns>True or False</returns>
		public Models.GeneralResponseModel<bool> EnviarMensagem(Models.MensagemEmailModel mensagem)
		{
			Models.GeneralResponseModel<bool> retorno = new Models.GeneralResponseModel<bool>();
			MailMessage mailMessage = new MailMessage();

			// Trocar por https://github.com/jstedfast/MailKit
			System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient
			{
				Host = this.host,
				Port = this.porta,
				Credentials = new System.Net.NetworkCredential(this.userName, this.pass),
				EnableSsl = false,
				Timeout = 10000
			};

			try
			{
				//From
				if (!string.IsNullOrEmpty(mensagem.De))
				{
					mailMessage.From = new MailAddress(mensagem.De);

					//Reply
					if (!string.IsNullOrEmpty(mensagem.Reply))
					{
						mailMessage.ReplyToList.Add(mensagem.Reply);
					}

					//To
					int tamanho = mensagem.Para.Length;
					if (tamanho > 0)
					{
						for (int i = 0; i < tamanho; i++)
						{
							if (!string.IsNullOrEmpty(mensagem.Para[i]))
							{
								mailMessage.To.Add(new MailAddress(mensagem.Para[i]));
							}
							else
							{
								retorno.Message = "Não foi passado pelo menos um destinatário para a mensagem!";
							}
						}

						//CC
						if (mensagem.Cc != null)
						{
							for (int i = 0; i < mensagem.Cc.Length; i++)
							{
								if (!string.IsNullOrEmpty(mensagem.Cc[i]))
								{
									mailMessage.CC.Add(new MailAddress(mensagem.Cc[i]));
								}
							}
						}

						//CCO
						if (mensagem.Cco != null)
						{
							for (int i = 0; i < mensagem.Cco.Length; i++)
							{
								if (!string.IsNullOrEmpty(mensagem.Cco[i]))
								{
									mailMessage.Bcc.Add(new MailAddress(mensagem.Cco[i]));
								}
							}
						}

						//Subject
						mailMessage.Subject = mensagem.Assunto;

						//Body
						mailMessage.IsBodyHtml = mensagem.IsHtml;
						mailMessage.BodyEncoding = System.Text.Encoding.Default;
						mailMessage.Priority = MailPriority.Normal;
						mailMessage.Body = mensagem.Corpo;

						//Envia a mensagem
						smtpClient.Send(mailMessage);

						retorno.Response = true;
					}
					else
					{
						retorno.Message = "Não foi passado pelo menos um destinatário para a mensagem!";
					}
				}
				else
				{
					retorno.Message = "Não foi passado um remetente para a mensagem!";
				}
			}
			// Testa se o email está criado
			catch (SmtpFailedRecipientsException sfre)
			{
				for (int i = 0; i < sfre.InnerExceptions.Length; i++)
				{
					SmtpStatusCode status = sfre.InnerExceptions[i].StatusCode;
					if (status == SmtpStatusCode.MailboxBusy ||
						status == SmtpStatusCode.MailboxUnavailable)
					{
						retorno.Message = "Delivery failed - retrying in 5 seconds.";
						System.Threading.Thread.Sleep(5000);
						smtpClient.Send(mailMessage);
					}
					else
					{
						retorno.Message = string.Format("Failed to deliver message to {0}", sfre.InnerExceptions[i].FailedRecipient);
					}
				}
			}
			catch (FormatException fex)
			{
				retorno.Message = fex.ToString();
			}
			catch (Exception ex)
			{
				retorno.Message = ex.ToString();
			}

			return retorno;
		}

		public Models.MensagemEmailModel MontarCabecalhoCorpoEmail(object dadosParaPreencherOCorpo)
		{
			Models.MensagemEmailModel mensagemEmailModel = new Models.MensagemEmailModel();

			// De
			mensagemEmailModel.De = "noreply@zambotecnologia.com.br";
			mensagemEmailModel.Reply = "noreply@zambotecnologia.com.br";
			// Para
			mensagemEmailModel.Para = new string[] { dadosParaPreencherOCorpo.Email };
			mensagemEmailModel.Cc = null;
			mensagemEmailModel.Cco = null;
			// Assunto
			mensagemEmailModel.Assunto = "Travel Ace - Cotação";

			// Corpo
			mensagemEmailModel.Corpo = this.MontarHtml(dadosParaPreencherOCorpo);
			mensagemEmailModel.IsHtml = true;

			return mensagemEmailModel;
		}

		private string MontarHtml(object dadosParaPreencherOCorpo)
		{
			StringBuilder html = new StringBuilder();
			html.Append("<!DOCTYPE html>");
			html.Append("<html lang='pt-br' style='position: relative;'>");
			html.Append("<head>");
			html.Append("<meta charset='utf-8'>");
			html.Append("<meta http-equiv='X-UA-Compatible' content='IE=edge'>");
			html.Append("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
			html.Append("</head>");
			html.Append("<body>");
			html.Append("</body>");
			html.Append("</html>");

			return html.ToString();
		}
	}
}

