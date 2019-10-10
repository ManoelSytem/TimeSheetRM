using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Text;
using TimeSheet.Domain;
using TimeSheet.Domain.Enty;
using TimeSheet.Domain.Util;

namespace TimeSheet.Infrastructure.Repository
{
    public class RMRepository
    {
        //base teste
        private const string ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=bgasha-scan)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=bahiagas)));User Id=rm;Password=rm;";
        private OracleConnection Conexao;


        public RMRepository()
        {
            Conexao = new OracleConnection(ConnectionString);
        }

        public CodDivergencia ObterCodigoDivergenciaPorCodigo(string cod)
        {
            try
            {
                var sql = $@"SELECT LTRIM(RTRIM(CODEVE)) as Constant, CODIGO as Codigo, DESCRICAO as Descricao FROM AABONO
                           WHERE CODIGO = LTRIM(RTRIM('{cod}'))";
                Conexao.Open();
                return Conexao.QueryFirstOrDefault<CodDivergencia>(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Conexao.Close();
            }
        }

        public IEnumerable<CodDivergencia> ObterListaCodigoDivergenciaPorIdDesc(string descId)
        {
            try
            {
                List<CodDivergencia> listCodivegencia = new List<CodDivergencia>();
                Conexao.Open();

                var sql = $@"SELECT LTRIM(RTRIM(CODEVE)) as Constant, CODIGO as Codigo, LTRIM(RTRIM(DESCRICAO)) as Descricao FROM AABONO
                                WHERE (DESCRICAO LIKE LTRIM(RTRIM('%{descId}%')) OR CODIGO LIKE LTRIM(RTRIM('%{descId}%'))) AND CODEVE IS NOT NULL AND CODEVE <> '04'";
                Conexao.Query<CodDivergencia>(sql);
                var QueryResult = Conexao.Query<CodDivergencia>(sql);

                foreach (CodDivergencia codigoResult in QueryResult)
                {
                    CodDivergencia novo = new CodDivergencia();
                    novo.codigo = codigoResult.codigo;
                    novo.Descricao = codigoResult.Descricao;
                    if (Constantes.HRSEXCEDENTES == codigoResult.Constant) { novo.Constant = "Hrs Excedentes"; }
                    else if (Constantes.ABONOS == codigoResult.Constant) { novo.Constant = "Abonos"; }
                    else if (Constantes.NAOTRABALHADA == codigoResult.Constant) { novo.Constant = "Não trabalhadas"; }

                    listCodivegencia.Add(novo);
                }

                return listCodivegencia;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                Conexao.Close();
            }
        }

        public Feriado ObterFeriado(string data, string filial)
        {
            var erro = "";
            Conexao.Open();
            try
            {
                if (filial.Length == 1)
                    filial = "0" + filial;


                var sqlUser = "";

                Feriado feriado = new Feriado();

                if (data.Length == 8) { 
                   sqlUser = $@"SELECT CODCALENDARIO as Filial, FERIADO AS Fixo, TO_CHAR(DIAFERIADO) AS DataFeriado, NOME AS Descricao FROM
                                 GFERIADO WHERE DIAFERIADO= TO_DATE('{data.ToDateProtheusReverseformate()}') AND CODCALENDARIO='{filial}'";
                }
                else
                {
                    sqlUser = $@"SELECT CODCALENDARIO as Filial, FERIADO AS Fixo, TO_CHAR(DIAFERIADO) AS DataFeriado, NOME AS Descricao FROM
                                 GFERIADO WHERE DIAFERIADO= TO_DATE('{data}') AND CODCALENDARIO='{filial}'";
                }

                var QueryResult = Conexao.Query<Feriado>(sqlUser);
                foreach (Feriado feriadoResult in QueryResult)
                {
                    feriado.DataFeriado = feriadoResult.DataFeriado;
                    feriado.Descricao = feriadoResult.Descricao;
                    feriado.Filial = feriadoResult.Filial;
                    feriado.Fixo = feriadoResult.Fixo;
                }
                return feriado;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                Conexao.Close();
            }

        }

        public List<Domain.Enty.Apontamento> ObterListBatidaDePontoDiario(string mat, string filial, string Data)
        {

            try
            {
                using (OracleConnection Conexao = new OracleConnection(ConnectionString))
                {

                    List<Apontamento> listApontamento = new List<Apontamento>();
                    Apontamento apontamento;

                    var sql = $@"SELECT LTRIM(RTRIM(TO_CHAR(round(ABATFUN.BATIDA/60,2)))) as hora
                                  FROM ABATFUN 
                                 WHERE ABATFUN.CODCOLIGADA=LTRIM(RTRIM('{filial}'))
                                 AND ABATFUN.CHAPA=LTRIM(RTRIM('{mat}')) 
                                 AND ABATFUN.DATA =LTRIM(RTRIM('{Data.ToDateProtheusReverseformate()}'))";
                    Conexao.Open();
                    var QueryResult = Conexao.Query<Apontamento>(sql);
                    foreach (Apontamento ApResult in QueryResult)
                    {
                        string valor = "";
                        apontamento = new Apontamento();
                        if (ApResult.hora.Length > 2)
                        {
                            int virgulaIndice = ApResult.hora.IndexOf(',');
                            valor = ApResult.hora.Substring(virgulaIndice);
                        }
                        if (ApResult.hora.Length == 1 | ApResult.hora.Length == 2) { apontamento.apontamento = TimeSpan.Parse("0" + ApResult.hora + ":00"); }
                        else if (ApResult.hora.Length >= 3) {

                            string hora = "";
                            int virgulaIndice = ApResult.hora.IndexOf(',');
                            var valorMinuto = ApResult.hora.Substring(virgulaIndice+1);
                            decimal minutos = Convert.ToDecimal("0,"+valorMinuto)*60;

                            if(ApResult.hora.Substring(0, virgulaIndice).Length ==2)
                            {
                                hora = ApResult.hora.Substring(0, 2);
                            }
                            else
                            {
                                hora = ApResult.hora.Substring(0, 1);
                            }
                           
                            decimal minutoRound = Math.Round(minutos);

                            string horaMinuto = hora + Convert.ToString(":" + Convert.ToString(minutoRound));
                            apontamento.apontamento = TimeSpan.Parse(horaMinuto);
                        }
                        else
                        {

                            apontamento.apontamento = TimeSpan.Parse(ApResult.hora.Replace(',', ':'));
                            apontamento.horaFim = TimeSpan.Parse(ApResult.hora.Replace(',', ':'));
                        }

                        listApontamento.Add(apontamento);
                    }

                    return listApontamento;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }

        }

        public Usuario ObterUsuarioPorMatricula(string mat)
        {
            Conexao.Open();
            try
            {
                Usuario usuario = new Usuario();
                var sqlUser = $@"Select NOME AS Nome from PFUNC
                          WHERE CHAPA = LTRIM(RTRIM('{mat}'))
                          ";
                var QueryResult = Conexao.Query<Usuario>(sqlUser);

                foreach (Usuario UserGerenciaResult in QueryResult)
                {
                    usuario.Nome = UserGerenciaResult.Nome;
                }

                return usuario;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Conexao.Close();
            }

        }


        public Usuario ObterListColaborador(string descricao, string centroDeCusto)
        {
            Conexao.Open();
            try
            {
                Usuario usuario = new Usuario();
                var sqlUser = $@" Select CHAPA as SubjectId, NOME AS Nome from PFUNC
                                  where (LTRIM(RTRIM(NOME)) LIKE UPPER('%{descricao}%') OR LTRIM(RTRIM(CHAPA)) = '{descricao}')";
                var QueryResult = Conexao.Query<Usuario>(sqlUser);

                foreach (Usuario UserResult in QueryResult)
                {
                    usuario.SubjectId = Convert.ToString(UserResult.Id);
                    usuario.Nome = UserResult.Nome;
                }

                return usuario;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Conexao.Close();
            }

        }

    }
}
