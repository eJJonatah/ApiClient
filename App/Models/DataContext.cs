using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiClient.App.Models
{
    public static class DataContext
    {
        public static class Interpreter
        {
            public static T Transpose<T>(object origin, Dictionary<string, string> relations) where T : class, new()
            {
                T RelatedTransposed = new T();
                var RelationsDetected = new Dictionary<string, string>();
                var JsonOrigin = origin as JObject;
                if (JsonOrigin != null) return null;
                var ValueData = RetrainData(JsonOrigin);
                foreach(var item in ValueData) 
                {
                    if (relations.ContainsValue(item.Key))
                    {
                        var related = new KeyValuePair<string, string>(
                                (from rel in relations
                                where rel.Value == item.Key
                                select rel).First().Key,
                                item.Key
                            );
                        RelationsDetected.Add(related.Key, related.Value);
                    }
                }
                foreach(var relation in RelationsDetected)
                {
                    RelatedTransposed.GetType()
                        .GetProperty(relation.Key)
                        .SetValue(RelatedTransposed, ValueData[relation.Key]);
                }
                return RelatedTransposed;
            }
            internal static Dictionary<string, object> RetrainData(JObject obj)
            {
                var validTypes = new JTokenType[] { JTokenType.String, JTokenType.Integer, JTokenType.Boolean };
                List<object> nestedObject_list = new List<object>();
                Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
                nestedObject_list = obj.Children().Select(x => (object)x).ToList();
            objectAnalysis:
                List<object> removedObjects = new List<object>();
                List<object> addingObjects = new List<object>();
                foreach (dynamic item in nestedObject_list)
                {
                    removedObjects.Add(item);
                    if (((IList<JTokenType>)validTypes).Contains(item.Type ?? JTokenType.Object))
                    {
                        keyValuePairs.Add(item.Path, item.Value);
                    }
                    else
                    {
                        foreach (dynamic nestedItem in item.Children())
                        {
                            addingObjects.Add(nestedItem);
                        }
                    }
                }
                nestedObject_list.AddRange(addingObjects);
                nestedObject_list = nestedObject_list.Except(removedObjects).ToList();
                if (nestedObject_list.Count > 0)
                    goto objectAnalysis;
                return keyValuePairs;
            }
            public static Dictionary<string, string> RetrainData(JArray obj)
            {
                List<object> nestedObject_list = new List<object>();
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                nestedObject_list = obj.OfType<dynamic>().ToList();
                int i = 0;
            objectAnalysis:
                List<object> removedObjects = new List<object>();
                List<object> addingObjects = new List<object>();
                i++;
                foreach (dynamic item in nestedObject_list)
                {
                    removedObjects.Add(item);
                    if (item.GetType().IsValueType)
                    {
                        keyValuePairs.Add(item.Key + i, item.Value.Value as string);
                    }
                    else
                    {
                        foreach (dynamic nestedItem in (JObject)item)
                        {
                            addingObjects.Add(nestedItem);
                        }
                    }
                }
                nestedObject_list.AddRange(addingObjects);
                nestedObject_list = nestedObject_list.Except(removedObjects).ToList();
                if (nestedObject_list.Count > 0)
                    goto objectAnalysis;
                return keyValuePairs;
            }
        }
    }
}
