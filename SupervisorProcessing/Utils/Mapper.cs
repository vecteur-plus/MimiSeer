using AutoMapper;
using MySqlX.XDevAPI.Relational;
using SupervisorProcessing.Dao;
using SupervisorProcessing.Model.Internal;
using System;
using System.Data.Common;

namespace SupervisorProcessing.Utils
{
    public class Mapper
    {
        private static MapperConfiguration Configuration = null;

        public static IMapper GetMapper()
        {
            if (Configuration == null)
            {
                Configuration = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<Guid, byte[]>().ConvertUsing(s => s.ToByteArray());

                    cfg.CreateMap<Schedule, CModelSchedule>()
                    .ForMember(dest => dest.IdSchedule, act => act.MapFrom(src => src.schedule_id_))
                    .ForMember(dest => dest.IdscheduleByte, act => act.MapFrom(src => src.schedule_id_))
                    .ForMember(dest => dest.SessionId, act => act.MapFrom(src => src.session_id_))
                    .ForMember(dest => dest.LastExistMessage, act => act.MapFrom(src => src.last_exit_message_ ?? "schedule en attente du lancement"))
                    .ForMember(dest => dest.LastRun, act => act.MapFrom(src => src.last_run_time_.HasValue ? src.last_run_time_.Value.ToString() : ""))
                    .ForMember(dest => dest.NextRun, act => act.MapFrom(src => src.next_run_time_.HasValue ? src.next_run_time_.Value.ToString() : ""))
                    .ForMember(dest => dest.IsPaused, act => act.MapFrom(src => src.is_paused_))
                    .ForMember(dest => dest.IsRunning, act => act.MapFrom(src => src.is_running_))
                    .ForMember(dest => dest.InputParameters, act => act.MapFrom(src => src.input_parameters_))
                    .ForMember(dest => dest.Cron, act => act.MapFrom(src => src.cron_))
                    .ForMember(dest => dest.StartTime, act => act.MapFrom(src => src.start_time_.HasValue ? src.start_time_.Value.ToString() : ""));              
                }
                );
            }

            return Configuration.CreateMapper();
        }
    }
}