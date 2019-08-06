﻿using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Text;
using TimeSheet.Domain.Enty;

namespace TimeSheet.Infrastructure.Repository
{
    public class MarcacaoRepository
    {
        private const string ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=BGASXL01)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ap12hml)));User Id=ap6;Password=ap6;";
        //private const string ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=bgasha-scan.intranet.bahiagas.com.br)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=BAHIAGAS)));User Id=ap6;Password=msbd106";

        public MarcacaoRepository()
        {

        }

        public void Add(Marcacao item)
        {
            try
            {
                using (OracleConnection dbConnection = new OracleConnection(ConnectionString))
                {
                    string sQuery = $@"INSERT INTO ZYZ010 (ZYZ_FILIAL, ZYZ_MATUSU, ZYZ_ANOMES, ZYZ_STATUS, ZYZ_CODINT,ZYZ_DTINIC,ZYZ_DTFINA, R_E_C_N_O_, R_E_C_D_E_L_)
                             VALUES('{item.Filial}', '{item.MatUsuario}', '{item.AnoMes}','{item.Status}', '{item.codigojornada}','{item.DataInicio}','{item.DataFim}', (SELECT NVL(MAX(X.R_E_C_N_O_),0)+1 FROM ZYZ010 X),0)";
                    dbConnection.Open();
                    dbConnection.Execute(sQuery);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Marcacao> ObterListaMarcacaoPorMatricula(string matricula)
        {
            try
            {
                List<Marcacao> listMarcacao = new List<Marcacao>();
                Marcacao marcacao;
                using (OracleConnection dbConnection = new OracleConnection(ConnectionString))
                {
                    string sQuery = $@"Select  LTRIM(RTRIM(ZYZ_DTINIC)) as DataInicio,  LTRIM(RTRIM(ZYZ_DTFINA)) as DataFim,  LTRIM(RTRIM(ZYZ_MATUSU)) as MatUsuario, LTRIM(RTRIM(ZYZ_CODIGO))  as Codigo, ZYZ_ANOMES as AnoMes, ZYZ_STATUS as Status from ZYZ010
                                        Where ZYZ_MATUSU = '{matricula}' AND D_E_L_E_T_ <> '*' ";
                    dbConnection.Open();
                    dbConnection.Execute(sQuery);

                    var QueryResult = dbConnection.Query<Marcacao>(sQuery);
                    foreach (Marcacao MarcacaoResult in QueryResult)
                    {
                        marcacao = new Marcacao();
                        marcacao.MatUsuario = MarcacaoResult.MatUsuario;
                        marcacao.AnoMes = MarcacaoResult.AnoMes;
                        marcacao.Status = MarcacaoResult.Status;
                        marcacao.Codigo = MarcacaoResult.Codigo;
                        marcacao.DataInicio = MarcacaoResult.DataInicio;
                        marcacao.DataFim = MarcacaoResult.DataFim;
                        listMarcacao.Add(marcacao);
                    }
                    return listMarcacao;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Marcacao> ObterListaMarcacaoPorCodigo(string codigo)
        {
            try
            {
                List<Marcacao> listMarcacao = new List<Marcacao>();
                Marcacao marcacao;
                using (OracleConnection dbConnection = new OracleConnection(ConnectionString))
                {
                    string sQuery = $@"Select  LTRIM(RTRIM(ZYZ_DTINIC)) as DataInicio,  LTRIM(RTRIM(ZYZ_DTFINA)) as DataFim, ZYZ_ANOMES as AnoMes, ZYZ_STATUS as Status from ZYZ010
                                        Where ZYZ_CODIGO = '{codigo}' AND  D_E_L_E_T_ <> '*'";
                    dbConnection.Open();
                    dbConnection.Execute(sQuery);

                    var QueryResult = dbConnection.Query<Marcacao>(sQuery);
                    foreach (Marcacao MarcacaoResult in QueryResult)
                    {
                        marcacao = new Marcacao();
                        marcacao.AnoMes = MarcacaoResult.AnoMes;
                        marcacao.Status = MarcacaoResult.Status;
                        marcacao.Codigo = MarcacaoResult.Codigo;
                        marcacao.DataInicio = MarcacaoResult.DataInicio;
                        marcacao.DataFim = MarcacaoResult.DataFim;
                        listMarcacao.Add(marcacao);
                    }
                    return listMarcacao;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Marcacao ObterMarcacaoPorCodigo(string codigo)
        {
            try
            {
                Marcacao Marcacao = new Marcacao();

                using (OracleConnection dbConnection = new OracleConnection(ConnectionString))
                {
                    string sQuery = $@"Select LTRIM(RTRIM(ZYZ_MATUSU)) as MatUsuario, LTRIM(RTRIM(ZYZ_FILIAL)) as Filial, LTRIM(RTRIM(ZYZ_DTINIC)) as DataInicio,  LTRIM(RTRIM(ZYZ_DTFINA)) as DataFim,  ZYZ_ANOMES as AnoMes, LTRIM(RTRIM(ZYZ_CODINT)) AS codigojornada, ZYZ_STATUS as Status from ZYZ010
                                        Where ZYZ_CODIGO = '{codigo}' AND  D_E_L_E_T_ <> '*'";
                    dbConnection.Open();
                    dbConnection.Execute(sQuery);

                    return dbConnection.QueryFirst<Marcacao>(sQuery);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


            public void Delete(string codigo)
            {

            using (OracleConnection dbConnection = new OracleConnection(ConnectionString))
            {
                dbConnection.Open();
                string sQuery = $@"UPDATE ZYZ010 
                                   SET D_E_L_E_T_ = '*',
                                   R_E_C_D_E_L_ = R_E_C_N_O_
                                   WHERE ZYZ_CODIGO = '{codigo}'";

                dbConnection.Execute(sQuery);

                string sQueryZ = $@" UPDATE ZYY010 
                     SET D_E_L_E_T_ = '*',
                     R_E_C_D_E_L_ = R_E_C_N_O_
                     WHERE ZYY_CODIGO = '{codigo}'";

                dbConnection.Execute(sQueryZ);
            }
            }

        public void UpdateStatusMarcacao(string codigo)
        {

            using (OracleConnection dbConnection = new OracleConnection(ConnectionString))
            {
                dbConnection.Open();
                string sQuery = $@"UPDATE ZYZ010 
                                   SET ZYZ_STATUS = '2'
                                   WHERE ZYZ_CODIGO = '{codigo}'";
                dbConnection.Execute(sQuery);
            }
        }

        public void UpdateFerias(string codigo, string ferias)
        {

            using (OracleConnection dbConnection = new OracleConnection(ConnectionString))
            {
                dbConnection.Open();
                string sQuery = $@"UPDATE ZYZ010 
                                   SET ZYZ_FER = '{ferias}'
                                   WHERE ZYZ_CODIGO = '{codigo}'";
                dbConnection.Execute(sQuery);
            }
        }


        public void SalvarCodigoFluig(string codigoMarcacao, string CodFluig)
        {
            using (OracleConnection dbConnection = new OracleConnection(ConnectionString))
            {
                dbConnection.Open();
                string sQuery = $@"UPDATE ZYZ010 
                                   SET ZYZ_CFLUIG = '{CodFluig}'
                                   WHERE ZYZ_CODIGO = '{codigoMarcacao}'";
                dbConnection.Execute(sQuery);
            }
        }

        public Marcacao ObterCodigoFluig(string codigoMarcacao)
        {
            using (OracleConnection dbConnection = new OracleConnection(ConnectionString))
            {
                dbConnection.Open();
                string sQuery = $@"Select LTRIM(RTRIM(ZYZ_CFLUIG)) as CodigoFluig, LTRIM(RTRIM(ZYZ_MATUSU)) as MatUsuario, LTRIM(RTRIM(ZYZ_FILIAL)) as Filial, LTRIM(RTRIM(ZYZ_DTINIC)) as DataInicio,  LTRIM(RTRIM(ZYZ_DTFINA)) as DataFim,  ZYZ_ANOMES as AnoMes, LTRIM(RTRIM(ZYZ_CODINT)) AS codigojornada, ZYZ_STATUS as Status from ZYZ010
                                        Where ZYZ_CODIGO = '{codigoMarcacao}' AND  D_E_L_E_T_ <> '*'";
                return dbConnection.QueryFirst<Marcacao>(sQuery);
            }
        }
    }
}
