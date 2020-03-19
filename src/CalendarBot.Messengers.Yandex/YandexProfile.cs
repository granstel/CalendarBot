using System;
using AutoMapper;
using CalendarBot.Models.Internal;
using Yandex.Dialogs.Models;
using Internal = CalendarBot.Models.Internal;
using YandexModels = Yandex.Dialogs.Models;

namespace CalendarBot.Messengers.Yandex
{
    /// <summary>
    /// Probably, registered at MappingModule of "Services" project
    /// </summary>
    public class YandexProfile : Profile
    {
        public YandexProfile()
        {
            CreateMap<InputModel, Internal.Request>()
                .ForMember(d => d.ChatHash, m => m.ResolveUsing(s => s.Session?.SkillId))
                .ForMember(d => d.UserHash, m => m.ResolveUsing(s => s.Session?.UserId))
                .ForMember(d => d.Text, m => m.ResolveUsing(s => s.Request?.OriginalUtterance))
                .ForMember(d => d.SessionId, m => m.ResolveUsing(s => s.Session?.SessionId))
                .ForMember(d => d.NewSession, m => m.ResolveUsing(s => s.Session?.New))
                .ForMember(d => d.Language, m => m.ResolveUsing(s => s.Meta?.Locale))
                .ForMember(d => d.Source, m => m.UseValue(Source.Yandex));

            CreateMap<Internal.Response, OutputModel>()
                .ForMember(d => d.Response, m => m.MapFrom(s => s))
                .ForMember(d => d.Session, m => m.MapFrom(s => s))
                .ForMember(d => d.Version, m => m.Ignore())
                .ForMember(d => d.StartAccountLinking, m => m.Ignore());

            CreateMap<Internal.Response, YandexModels.Response>()
                .ForMember(d => d.Text, m => m.MapFrom(s => s.Text.Replace(Environment.NewLine, "\n")))
                .ForMember(d => d.Tts, m => m.MapFrom(s => s.AlternativeText.Replace(Environment.NewLine, "\n")))
                .ForMember(d => d.EndSession, m => m.MapFrom(s => s.Finished))
                .ForMember(d => d.Buttons, m => m.Ignore());

            CreateMap<Internal.Response, Session>()
                .ForMember(d => d.UserId, m => m.MapFrom(s => s.UserHash))
                .ForMember(d => d.MessageId, m => m.Ignore())
                .ForMember(d => d.SessionId, m => m.Ignore());

            CreateMap<InputModel, OutputModel>()
                .ForMember(d => d.Session, m => m.MapFrom(s => s.Session))
                .ForMember(d => d.Version, m => m.MapFrom(s => s.Version))
                .ForMember(d => d.Response, m => m.Ignore())
                .ForMember(d => d.StartAccountLinking, m => m.Ignore());
        }
    }
}
