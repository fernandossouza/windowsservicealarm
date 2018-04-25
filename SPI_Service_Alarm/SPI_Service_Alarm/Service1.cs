using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using System.Threading;

namespace SPI_Service_Alarm
{
    public partial class Service1 : ServiceBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Thread threadSecundaria;
        Logic logicService;

        public Service1()
        {
            try
            {
                InitializeComponent();
                logicService = new Logic();
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        protected override void OnStart(string[] args)
        {
            log.Info("Iniciando Serviço V1.0");
            threadSecundaria = new Thread(x => logicService.IniciarProcesso());
            threadSecundaria.Start();
        }

        /// <summary>
        /// Para permitir que o serviço seja iniciado em modo DEBUG
        /// </summary>
        /// <param name="args"></param>
        public void OnStartInDebug(string[] args)
        {
            OnStart(args);
        }

        protected override void OnStop()
        {
            log.Info("Parando Serviço");
            log.Info("Aguarde ....");
            logicService.ServicoAtivo = false;

            while (threadSecundaria.IsAlive) ;
            log.Info("Serviço Encerrado ....");
        }
    }
}
