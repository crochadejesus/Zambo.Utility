//
//  MensagemBll.cs
//
//  Autor: Cláudio Rocha de Jesus <crochadejesus@zambotecnologia.com.br>
//  Criado em: 24/05/2016 16:17
//
//  Copyright (c) 2016 Zambo Tecnologia Ltda
//

using System;
using System.Net;
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
			this.host = "hermes.travelace.com.br";
			this.userName = "cotacao@travelace.com.br";
			this.pass = "";
			this.porta = 465;
		}

		/// <summary>
		/// Caso deseje utilizar um servidor de email diferente ou se autenticar utilizando outro endereço de email
		/// </summary>
		/// <param name="servidor">Endereço do servidor. Ex: sheol.travelace.com.br</param>
		/// <param name="usuario">Endereço de email de uma conta válida. Ex: cjesus@travelace.com.br</param>
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
		public Models.RetornoAcaoModel EnviarMensagem(Models.MensagemEmailModel mensagem)
		{
			Models.RetornoAcaoModel retorno = new Models.RetornoAcaoModel();

			try
			{
				SmtpClient smtpClient = new SmtpClient(this.host);
				NetworkCredential basicCredential = new NetworkCredential(this.userName, this.pass);
				smtpClient.Credentials = basicCredential;
				smtpClient.EnableSsl = true;
				smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
				smtpClient.Port = this.porta;

				MailMessage message = new MailMessage();
				message.IsBodyHtml = mensagem.IsHtml;
				message.Priority = MailPriority.High;
				message.BodyEncoding = System.Text.Encoding.Default;

				//From
				if (!string.IsNullOrEmpty(mensagem.De))
				{
					message.From = new MailAddress(mensagem.De);

					//Reply
					if (!string.IsNullOrEmpty(mensagem.Reply))
					{
						message.ReplyToList.Add(mensagem.Reply);
					}

					//To
					if (mensagem.Para.Length != 0)
					{
						for (byte i = 0; i < mensagem.Para.Length; ++i)
						{
							if (!string.IsNullOrEmpty(Convert.ToString(mensagem.Para[i])))
							{
								message.To.Add(mensagem.Para[i]);
							}
						}

						//CC
						for (byte i = 0; i < mensagem.Cc.Length; ++i)
						{
							if (!string.IsNullOrEmpty(Convert.ToString(mensagem.Cc[i])))
							{
								message.CC.Add(mensagem.Cc[i]);
							}
						}

						//CCO
						for (byte i = 0; i < mensagem.Cco.Length; ++i)
						{
							if (!string.IsNullOrEmpty(Convert.ToString(mensagem.Cco[i])))
							{
								message.Bcc.Add(mensagem.Cco[i]);
							}
						}

						//Subject
						message.Subject = mensagem.Assunto;

						//Body
						message.IsBodyHtml = mensagem.IsHtml;
						message.BodyEncoding = System.Text.Encoding.Default;
						message.Body = mensagem.Corpo;

						//Envia a mensagem
						smtpClient.Send(message);

						retorno.Resultado = true;
					}
					else
					{
						retorno.Mensagem = "Não foi passado pelo menos um destinatário para a mensagem!";
					}
				}
				else
				{
					retorno.Mensagem = "Não foi passado um remetente para a mensagem!";
				}
			}
			// Testa se o email está criado
			catch (SmtpFailedRecipientException sfre)
			{
				retorno.Mensagem = sfre.Message;
				return retorno;
			}
			catch (FormatException fex)
			{
				retorno.Mensagem = fex.Message;
				return retorno;
			}
			catch (Exception ex)
			{
				retorno.Mensagem = ex.Message;
				return retorno;
			}

			return retorno;
		}

		public Models.MensagemEmailModel MontarCabecalhoCorpoEmail(Models.DadosTitularModel dadosTitularModel,
		                                                           Models.SimulacaoCompraRequestModel simulacaoCompraRequestModel,
		                                                           Models.SimulacaoCompraModel simulacaoCompraModel)
		{
			Models.MensagemEmailModel mensagemEmailModel = new Models.MensagemEmailModel();

			// De
			mensagemEmailModel.De = "cotacao@travelace.com.br";
			mensagemEmailModel.Reply = string.Empty;
			// Para
			mensagemEmailModel.Para = new string[] {dadosTitularModel.Email};
			mensagemEmailModel.Cc = new string[0];
			mensagemEmailModel.Cco = new string[0];
			// Assunto
			mensagemEmailModel.Assunto = "Travel Ace - Cotação";

			// Corpo
			mensagemEmailModel.Corpo = this.MontarHtml(dadosTitularModel, simulacaoCompraRequestModel, simulacaoCompraModel);
			mensagemEmailModel.IsHtml = true;

			return mensagemEmailModel;
    	}

	    string MontarHtml(Models.DadosTitularModel dadosTitularModel,
		                          Models.SimulacaoCompraRequestModel simulacaoCompraRequestModel,
								  Models.SimulacaoCompraModel simulacaoCompraModel)
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
			html.Append("<div class='rcmBody'>");
			html.Append("<table id='CobertGeral' style='margin: 0 auto' class='stripe row-border order-column' cellspacing='0' width='1024'>");
			html.Append("<thead>");
			html.Append("<tr>");
			html.Append("<td colspan='4'>");
			html.Append("<img style='border-bottom: 3px solid #464a88' src='https://intravel.travelace.com.br/Content/img/topo-email-1024x768.jpg' alt='topo travelace'>");
			html.Append("</td>");
			html.Append("</tr>");
			html.Append("<tr>");
			html.Append("<td>");
			html.Append("<br>");
			html.Append("</td>");
			html.Append("</tr>");
			html.Append("<tr>");
			html.Append("<td colspan='4' style='background-color: #ffffff'>");
			html.Append("Prezado <strong>" + dadosTitularModel.NomeCompleto + "</strong>,");
			html.Append("</td>");
			html.Append("</tr>");
			html.Append("<tr>");
			html.Append("<td colspan='4' style='background-color: #ffffff; height: 30px' class='azulTabNew'>");
			html.Append("Abaixo a cotação dos produtos para o(s) destino(s) ");
			html.Append("<strong style='font-weight: bold'>" + simulacaoCompraRequestModel.Destino + "</strong>, para ");
			html.Append("<strong>" + simulacaoCompraRequestModel.QtdPax + " passageiros</strong>, com o periodo de viagem do dia ");
			html.Append("<strong style='font-weight: bold'>" + simulacaoCompraRequestModel.DataIda + "</strong> até ");
			html.Append("<strong style='font-weight: bold'>" + simulacaoCompraRequestModel.DataVolta + "</strong>, totalizando ");
			html.Append("<strong style='font-weight: bold'>" + simulacaoCompraModel.DiasViagem + " dias.</strong>");
			html.Append("</td>");
			html.Append("</tr>");
			html.Append("<tr>");
			html.Append("<td colspan='4' style='background-color: #ffffff; height: 30px'>");
			html.Append("<h4 style='margin: 0 0 10px 0; font-weight: lighter'>");
			html.Append("Cotação valida somente para a data de hoje ");
			html.Append("<strong style='font-weight: bold'>" + System.DateTime.Today.ToString("dd/MM/yyyy") + "</strong> | Câmbio do Dia: ");
			html.Append("<strong>" + simulacaoCompraModel.ValorDoCambio + "</strong>");
			html.Append("</h4>");
			html.Append("</td>");
			html.Append("</tr>");
			html.Append("<tr>");
			html.Append("<td>");
			html.Append("<br>");
			html.Append("</td>");
			html.Append("</tr>");
			html.Append("<tr style='height: 40px'>");
			html.Append("<th style='background-color: #d8e1f0; color: #464a88; text-align: left !important; width: 200px'>Cobertura</th>");
			html.Append("<th style='background-color: #d8e1f0; color: #464a88; text-align: center; width: 150px' class='centertxt nameProduct'>" + simulacaoCompraModel.ProdutosRecomendados.ProdutoMaiorValor.Descricao + "</th>");
			html.Append("<th style='background-color: #d8e1f0; color: #464a88; text-align: center; width: 150px' class='centertxt nameProduct'>" + simulacaoCompraModel.ProdutosRecomendados.ProdutoValorMedio.Descricao + "</th>");
			html.Append("<th style='background-color: #d8e1f0; color: #464a88; text-align: center; width: 150px' class='centertxt nameProduct'>" + simulacaoCompraModel.ProdutosRecomendados.ProdutoMenorValor.Descricao + "</th>");
			html.Append("</tr>");
			html.Append("<tr>");
			html.Append("<th colspan='4' style='color: #ffffff; background-color: #464a88; text-align: left !important' class='pretoTabNew TabNewFont'>Descrição dos Serviços e Limites de Valores</th>");
			html.Append("</tr>");
			html.Append("<tr>");
			html.Append("<th></th>");
			html.Append("<th></th>");
			html.Append("<th></th>");
			html.Append("</tr>");
			html.Append("</thead>");
			html.Append("<tbody>");
				var tamanhoBeneficios = simulacaoCompraModel.Beneficios.Length;
				for (int i = 0; i < tamanhoBeneficios; i++) 
				{
					var idBeneficio = simulacaoCompraModel.Beneficios[i].IdBeneficio;
					int resto = i % 2;
					if (resto == 0) // par
					{
						html.Append("<tr style='height: 40px' bgcolor='#cccccc'>");
						html.Append("<td style='border: 1px solid #ffffff'>" + simulacaoCompraModel.Beneficios[i].Descricao + "</td>");
						var tamanhoBeneficiosMaiorValor = simulacaoCompraModel.ProdutosRecomendados.ProdutoMaiorValor.Beneficios.Length;
						uint contadorBeneficiosMaiorValor = 0;
					    for (int j = 0; j < tamanhoBeneficiosMaiorValor; j++)
						{
							var idBeneficioProduto = simulacaoCompraModel.ProdutosRecomendados.ProdutoMaiorValor.Beneficios[j].IdBeneficio;
							if (idBeneficio == idBeneficioProduto)
							{
								contadorBeneficiosMaiorValor++;
								var valor = simulacaoCompraModel.ProdutosRecomendados.ProdutoMaiorValor.Beneficios[j].Valor;
								if (!string.IsNullOrWhiteSpace(valor))
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'>" + valor + "</td>");
								}
								else 
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'> - </td>");
								}

								break;
							} else if (j == (tamanhoBeneficiosMaiorValor - 1)) // Testará se nenhum dos benefícios coincindio
							{
								if (contadorBeneficiosMaiorValor == 0)
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'> - </td>");
								}
							}
						}

						var tamanhoBeneficiosValorMedio = simulacaoCompraModel.ProdutosRecomendados.ProdutoValorMedio.Beneficios.Length;
						uint contadorBeneficiosValorMedio = 0;
						for (int j = 0; j < tamanhoBeneficiosValorMedio; j++)
						{
							var idBeneficioProduto = simulacaoCompraModel.ProdutosRecomendados.ProdutoValorMedio.Beneficios[j].IdBeneficio;
							if (idBeneficio == idBeneficioProduto)
							{
								contadorBeneficiosValorMedio++;
								var valor = simulacaoCompraModel.ProdutosRecomendados.ProdutoValorMedio.Beneficios[j].Valor;
								if (!string.IsNullOrWhiteSpace(valor))
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'>" + valor + "</td>");
								}
								else
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'> - </td>");
								}

								break;
							}
							else if (j == (tamanhoBeneficiosValorMedio - 1)) // Testará se nenhum dos benefícios coincindio
							{
								if (contadorBeneficiosValorMedio == 0)
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'> - </td>");
								}
							}
	 					}

						var tamanhoBeneficiosMenorValor = simulacaoCompraModel.ProdutosRecomendados.ProdutoMenorValor.Beneficios.Length;
						uint contadorBeneficiosMenorValor = 0;
						for (int j = 0; j < tamanhoBeneficiosMenorValor; j++)
						{
							var idBeneficioProduto = simulacaoCompraModel.ProdutosRecomendados.ProdutoMenorValor.Beneficios[j].IdBeneficio;
							if (idBeneficio == idBeneficioProduto)
							{
								contadorBeneficiosMenorValor++;
								var valor = simulacaoCompraModel.ProdutosRecomendados.ProdutoMenorValor.Beneficios[j].Valor;
								if (!string.IsNullOrWhiteSpace(valor))
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'>" + valor + "</td>");
								}
								else
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'> - </td>");
								}

								break;
							}
							else if (j == (tamanhoBeneficiosMenorValor - 1)) // Testará se nenhum dos benefícios coincindio
							{
								if (contadorBeneficiosMenorValor == 0)
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'> - </td>");
								}
							}
	 					}
						html.Append("</tr>");
					}
					else // impar
					{
						html.Append("<tr style='height: 40px' bgcolor='#e5e5e5'>");
						html.Append("<td style='border: 1px solid #ffffff'>" + simulacaoCompraModel.Beneficios[i].Descricao + "</td>");
						var tamanhoBeneficiosMaiorValor = simulacaoCompraModel.ProdutosRecomendados.ProdutoMaiorValor.Beneficios.Length;
						uint contadorBeneficiosMaiorValor = 0;
						for (int j = 0; j < tamanhoBeneficiosMaiorValor; j++)
						{
							var idBeneficioProduto = simulacaoCompraModel.ProdutosRecomendados.ProdutoMaiorValor.Beneficios[j].IdBeneficio;
							if (idBeneficio == idBeneficioProduto)
							{
								contadorBeneficiosMaiorValor++;
								var valor = simulacaoCompraModel.ProdutosRecomendados.ProdutoMaiorValor.Beneficios[j].Valor;
								if (!string.IsNullOrWhiteSpace(valor))
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'>" + valor + "</td>");
								}
								else
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'> - </td>");
								}

								break;
							}
							else if (j == (tamanhoBeneficiosMaiorValor - 1)) // Testará se nenhum dos benefícios coincindio
							{
								if (contadorBeneficiosMaiorValor == 0)
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'> - </td>");
								}
							}
						}

						var tamanhoBeneficiosValorMedio = simulacaoCompraModel.ProdutosRecomendados.ProdutoValorMedio.Beneficios.Length;
						uint contadorBeneficiosValorMedio = 0;
						for (int j = 0; j < tamanhoBeneficiosValorMedio; j++)
						{
							var idBeneficioProduto = simulacaoCompraModel.ProdutosRecomendados.ProdutoValorMedio.Beneficios[j].IdBeneficio;
							if (idBeneficio == idBeneficioProduto)
							{
								contadorBeneficiosValorMedio++;
								var valor = simulacaoCompraModel.ProdutosRecomendados.ProdutoValorMedio.Beneficios[j].Valor;
								if (!string.IsNullOrWhiteSpace(valor))
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'>" + valor + "</td>");
								}
								else
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'> - </td>");
								}

								break;
							}
							else if (j == (tamanhoBeneficiosValorMedio - 1)) // Testará se nenhum dos benefícios coincindio
							{
								if (contadorBeneficiosValorMedio == 0)
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'> - </td>");
								}
							}
						}

						var tamanhoBeneficiosMenorValor = simulacaoCompraModel.ProdutosRecomendados.ProdutoMenorValor.Beneficios.Length;
						uint contadorBeneficiosMenorValor = 0;
						for (int j = 0; j < tamanhoBeneficiosMenorValor; j++)
						{
							var idBeneficioProduto = simulacaoCompraModel.ProdutosRecomendados.ProdutoMenorValor.Beneficios[j].IdBeneficio;
							if (idBeneficio == idBeneficioProduto)
							{
								contadorBeneficiosMenorValor++;
								var valor = simulacaoCompraModel.ProdutosRecomendados.ProdutoMenorValor.Beneficios[j].Valor;
								if (!string.IsNullOrWhiteSpace(valor))
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'>" + valor + "</td>");
								}
								else
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'> - </td>");
								}

								break;
							}
							else if (j == (tamanhoBeneficiosMenorValor - 1)) // Testará se nenhum dos benefícios coincindio
							{
								if (contadorBeneficiosMenorValor == 0)
								{
									html.Append("<td style='text-align: center; border: 1px solid #ffffff' class='center'> - </td>");
								}
							}
						}
						html.Append("</tr>");
					}
				}
			html.Append("</tbody>");
			html.Append("<tfoot class='tFootMod'>");
			html.Append("<tr id='LoopUpgVal'>");
			html.Append("<td class='whiteline'>Valor do Produto + Benefício</td>");
			html.Append("<td style='font-size: 17px; color: #ffffff; text-align: center; background-color: #00589a; height: 50px; border: 1px solid #ffffff' class='centertxt ValorSizeBen FiftyGetVal'>");
			html.Append("<span>R$ " + simulacaoCompraModel.ProdutosRecomendados.ProdutoMaiorValor.Tarifa.Valor + "/ US$ " + simulacaoCompraModel.ProdutosRecomendados.ProdutoMaiorValor.Tarifa.ValorMoeda + "</span>");
			html.Append("</td>");
			html.Append("<td style='font-size: 17px; color: #ffffff; text-align: center; background-color: #00589a; height: 50px; border: 1px solid #ffffff' class='centertxt ValorSizeBen FiftyGetVal'>");
			html.Append("<span>R$ " + simulacaoCompraModel.ProdutosRecomendados.ProdutoValorMedio.Tarifa.Valor + "/ US$ " + simulacaoCompraModel.ProdutosRecomendados.ProdutoValorMedio.Tarifa.ValorMoeda + "</span>");
			html.Append("</td>");
			html.Append("<td style='font-size: 17px; color: #ffffff; text-align: center; background-color: #00589a; height: 50px; border: 1px solid #ffffff' class='centertxt ValorSizeBen FiftyGetVal'>");
			html.Append("<span>R$ " + simulacaoCompraModel.ProdutosRecomendados.ProdutoMenorValor.Tarifa.Valor + "/ US$ " + simulacaoCompraModel.ProdutosRecomendados.ProdutoMenorValor.Tarifa.ValorMoeda + "</span>");
			html.Append("</td>");
			html.Append("</tr>");
			html.Append("<tr>");
			html.Append("<th colspan='4' style='color: #ffffff; background-color: #464a88; text-align: left !important; height: 50px' class='pretoTabNew TabNewFont'></th>");
			html.Append("</tr>");
			html.Append("</tfoot>");
			html.Append("</table>");
			html.Append("<br>");
			html.Append("Atenciosamente, ");
			html.Append("<br>");
			html.Append("<br>");
			html.Append("<strong>Travel Ace Assistancia ao Viajante</strong>");
			html.Append("<br>");
			html.Append("<strong>Email:</strong><a href='mailto:cotacao@travelace.com.br' rel='noreferrer'>cotacao@travelace.com.br</a>");
			html.Append("<br>");
			html.Append("</div>");
			html.Append("</body>");
			html.Append("</html>");

			return html.ToString();
		}
	}
}

