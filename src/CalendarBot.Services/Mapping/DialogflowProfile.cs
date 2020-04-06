﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf.WellKnownTypes;
using CalendarBot.Models.Internal;
using System;
using GranSteL.Helpers.Redis.Extensions;
using System.Reflection;

namespace CalendarBot.Services.Mapping
{
    public class DialogflowProfile : Profile
    {
        public DialogflowProfile()
        {
            CreateMap<QueryResult, Dialog>()
                .ForMember(d => d.Parameters, m => m.MapFrom(s => GetParameters(s)))
                .ForMember(d => d.Payloads, m => m.MapFrom(s => GetPayloads(s)))
                .ForMember(d => d.Response, m => m.MapFrom(s => s.FulfillmentText))
                .ForMember(d => d.ParametersIncomplete, m => m.MapFrom(s => !s.AllRequiredParamsPresent))
                .ForMember(d => d.Action, m => m.MapFrom(s => s.Action))
                .ForMember(d => d.EndConversation, m => m.Ignore())
                .AfterMap((s, d) =>
                {
                    d.EndConversation = s.DiagnosticInfo?.Fields?.Where(f => string.Equals(f.Key, "end_conversation"))
                                            .Select(f => f.Value.BoolValue).FirstOrDefault() ?? false;
                });
        }

        private IDictionary<string, string> GetParameters(QueryResult queryResult)
        {
            var dictionary = new Dictionary<string, string>();

            var fields = queryResult?.Parameters.Fields;

            if (fields?.Any() != true)
            {
                return dictionary;
            }

            foreach (var field in fields)
            {
                if (field.Value.KindCase == Value.KindOneofCase.StringValue)
                {
                    dictionary.Add(field.Key, field.Value.StringValue);
                }
                else if (field.Value.KindCase == Value.KindOneofCase.StructValue)
                {
                    var stringValues = new List<string>();

                    foreach (var valueField in field.Value.StructValue.Fields)
                    {
                        if (valueField.Value.KindCase == Value.KindOneofCase.StringValue)
                        {
                            stringValues.Add(valueField.Value.StringValue);
                        }
                    }

                    var stringValue = string.Join("/", stringValues);

                    dictionary.Add(field.Key, stringValue);
                }
                else if (field.Value.KindCase == Value.KindOneofCase.ListValue)
                {
                    var stringValues = new List<string>();

                    foreach (var valueField in field.Value.ListValue.Values)
                    {
                        if (valueField.KindCase == Value.KindOneofCase.StringValue)
                        {
                            stringValues.Add(valueField.StringValue);
                        }
                    }

                    var stringValue = string.Join("/", stringValues);

                    dictionary.Add(field.Key, stringValue);
                }
            }

            return dictionary;
        }

        private ICollection<Payload> GetPayloads(QueryResult queryResult)
        {
            var result = new List<Payload>();

            var payloadType = queryResult?.Parameters.Fields.Where(f => string.Equals("payloadType", f.Key)).Select(f => f.Value?.StringValue).FirstOrDefault();

            if(string.IsNullOrEmpty(payloadType))
            {
                return result;
            }

            switch (payloadType)
            {
                case "AnswerTemplate":
                    var sourcePayloads = queryResult?.FulfillmentMessages?
                                    .Where(m => m.MessageCase == Intent.Types.Message.MessageOneofCase.Payload)
                                    .Select(m => m.Payload.ToString().Deserialize<AnswerTemplate>()).ToList();

                    result.AddRange(sourcePayloads);
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
