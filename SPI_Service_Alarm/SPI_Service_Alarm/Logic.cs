using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using System.Configuration;
using SPI_Service_Alarm.Model;
using System.Collections;

namespace SPI_Service_Alarm
{
    class Logic
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private int _TempoProcesso = 1000;
        private List<Alarm> _alarmesAtivosList = null;
        private List<Tag> _tagsList = null;
        public bool ServicoAtivo = true;
        private readonly HttpRequestOtherAPI _httpOtherAPI;
        private readonly ILPostgreSQL _ilPostgreSQL;

        public Logic()
        {
            _httpOtherAPI = new HttpRequestOtherAPI();
            _ilPostgreSQL = new ILPostgreSQL();
        }

        public void IniciarProcesso()
        {
            //Configurando o serviço
            while(!ConfigurandoServico())
            {
                _log.Error("Erro na configuração do serviço !!!");
                System.Threading.Thread.Sleep(1000);
            }

            while(ServicoAtivo)
            {
                VerificaAlarme();
                System.Threading.Thread.Sleep(_TempoProcesso);
            }
        }

        private void VerificaAlarme()
        {
            var tagsIL = _ilPostgreSQL.SelectDB(_tagsList);
            DateTime dateNow = DateTime.Now;

            foreach(var ilTag in tagsIL)
            {
                var alarmeAtivo = ConverteAlarmeAtivo(ilTag);

                var alarmeNaAPi = _alarmesAtivosList.Where(x => x.tagIL.ToString().ToLower() == ilTag.TagName.ToLower()).FirstOrDefault();

                if (alarmeNaAPi == null ||
                    alarmeNaAPi.alarmDescription == null ||
                    alarmeNaAPi.alarmDescription.ToLower() != alarmeAtivo.alarmDescription.ToLower())
                {
                    var tagDb = _tagsList.Where(x => x.physicalTag.ToLower() == ilTag.TagName.ToLower()).FirstOrDefault();

                    var thingGroup = _httpOtherAPI.GetAPIThingGroup(tagDb.thingGroupId);

                    alarmeAtivo.alarmName = tagDb.tagName;
                    alarmeAtivo.tagIL = ilTag.TagName;
                    alarmeAtivo.thingId = thingGroup.thingsIds.FirstOrDefault();
                    alarmeAtivo.datetime = dateNow.Ticks;


                    //adicionando alarme na API

                    var alarmReturn = _httpOtherAPI.PostAPIAlarm(alarmeAtivo);

                    if(alarmReturn == null)
                    {
                        _log.Error("Erro ao tentar adicionar o alarme na API");
                        continue;
                    }

                    if(alarmeNaAPi != null)
                    {
                        _alarmesAtivosList.Remove(alarmeNaAPi);
                    }

                    _alarmesAtivosList.Add(alarmeAtivo);

                }
                
            }
        }

        private Alarm ConverteAlarmeAtivo(TagIL ilTag)
        {
            Alarm alarmeAtivo = new Alarm();

            if (ilTag.TagValue.Trim().ToLower() == "_desc")
            {
                alarmeAtivo.alarmDescription = "Offline";
                alarmeAtivo.priority = 3;
                alarmeAtivo.alarmColor = "#969995";
            }
            else
            {
                try
                {
                    int valorInt = Convert.ToInt32(ilTag.TagValue);
                    //Convertendo em bit
                    BitArray Alarmbit = new BitArray(new int[] { valorInt });
                    if (Alarmbit[5] == true || Alarmbit[4] == true)
                    {
                        alarmeAtivo.alarmDescription = "Ok";
                        alarmeAtivo.priority = 0;
                        alarmeAtivo.alarmColor = "#0fa301";
                    }                    
                    if(Alarmbit[0]==true || Alarmbit[1]==true)
                    {
                        alarmeAtivo.alarmDescription = "Alto";
                        alarmeAtivo.priority = 1;
                        alarmeAtivo.alarmColor = "#ffff00";
                    }
                    if (Alarmbit[6] == true || Alarmbit[7] == true)
                    {
                        alarmeAtivo.alarmDescription = "Baixo";
                        alarmeAtivo.priority = 1;
                        alarmeAtivo.alarmColor = "#ffff00";
                    }
                    if (Alarmbit[2] == true || Alarmbit[3] == true)
                    {
                        alarmeAtivo.alarmDescription = "Muito Alto";
                        alarmeAtivo.priority = 2;
                        alarmeAtivo.alarmColor = "#ed0404";
                    }
                    if (Alarmbit[8] == true || Alarmbit[9] == true)
                    {
                        alarmeAtivo.alarmDescription = "Muito Baixo";
                        alarmeAtivo.priority = 2;
                        alarmeAtivo.alarmColor = "#ed0404";
                    }
                }
                catch(Exception ex)
                {
                    _log.Error("Erro na conversão de inteiro para bit no alarme da tag: " + ilTag.TagName);
                    _log.Error(ex.Message);
                    alarmeAtivo.alarmDescription = "ERRO";
                    alarmeAtivo.priority = 3;
                    alarmeAtivo.alarmColor = "#969995";
                }

            }

            return alarmeAtivo;
        }

        private bool ConfigurandoServico()
        {
            try
            {
                _log.Debug("Configurando serviço");
                _log.Debug("Tempo de Processo");
                _TempoProcesso = Convert.ToInt32(ConfigurationManager.AppSettings["TempoProcesso"]);

                if (CriarListaAlarmes())
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return false;
            }
        }

        private bool CriarListaAlarmes()
        {
            try
            {
                _alarmesAtivosList = new List<Alarm>();

                AtualizaListaTags();

                _log.Debug("Criando os objetos de monitoramento");


                ThingGroup thingGroup = null;
                ThingAlarm thingAlarm = null;

                foreach (var tag in _tagsList.OrderBy(x => x.thingGroupId))
                {

                    if (thingGroup == null || thingGroup.thingGroupId != tag.thingGroupId)
                    {
                        thingGroup = _httpOtherAPI.GetAPIThingGroup(tag.thingGroupId);
                        thingAlarm = _httpOtherAPI.GetAPIAlarm(thingGroup.thingsIds.FirstOrDefault());
                    }

                    Alarm alarm = thingAlarm.alarms.Where(x => x.alarmName == tag.tagName).FirstOrDefault();

                    if (alarm == null)
                    {
                        alarm = new Alarm();
                        alarm.thingId = thingGroup.thingsIds.FirstOrDefault();
                        
                    }
                    alarm.tagIL = tag.physicalTag;

                    _alarmesAtivosList.Add(alarm);
                }

                return true;
            }
            catch(Exception ex)
            {
                _log.Error(ex.Message);
                return false;
            }
        }

        private void AtualizaListaTags()
        {

            _log.Debug("Get tags de alarmes configurado no sistema");
            _tagsList = new List<Tag>();
            _tagsList = _httpOtherAPI.GetAPITagsAlarme();
        }


    }
}
