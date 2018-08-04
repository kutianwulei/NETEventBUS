/****************************************************************
** �ļ���:   EventBus.cs
** ��Ҫ��:   EventBus��  
** 
** ������:   
** ��  ��:   2017.3.10
** �޸���:   
** ��  ��:   
** �޸����ݣ� 
** ��  ��:  
** ��  ��:   
** ��  ע:   
****************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace HT.EventBus.Core
{

    public class EventBus
    {
        /// <summary>
        /// �¼����߶���
        /// </summary>
        /// 

        private static EventBus _eventBus = null;
        public delegate void OnEvent(object sender,bool b,Exception ex);
        public event OnEvent onEvent;
    
        /// <summary>
        /// ����ģ���¼�����ֵ䣬���ڴ洢����ģ�͵ľ��
        /// </summary>
        private static Dictionary<Type, List<object>> _dicEventHandler = new Dictionary<Type, List<object>>();

        /// <summary>
        /// ��������ģ�ʹ�����ʱ����ס
        /// </summary>
        private readonly object _syncObject = new object();

        /// <summary>
        /// �����¼�����
        /// </summary>
        public static EventBus Instance
        {
            get
            {
                return _eventBus ?? (_eventBus = new EventBus());
            }
        }

        /// <summary>
        /// ͨ���أͣ��ļ���ʼ���¼����ߣ����������ڣأͣ�������
        /// </summary>
        /// <returns></returns>
        public static EventBus InstanceForXml()
        {
            if (_eventBus == null)
            {
                XElement root = XElement.Load(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EventBus.xml"));
                foreach (var evt in root.Elements("Event"))
                {
                    List<object> handlers = new List<object>();

                    Type publishEventType = Type.GetType(evt.Element("PublishEvent").Value);
                    foreach (var subscritedEvt in evt.Elements("SubscribedEvents"))
                        foreach (var concreteEvt in subscritedEvt.Elements("SubscribedEvent"))
                            handlers.Add(Type.GetType(concreteEvt.Value));

                    _dicEventHandler[publishEventType] = handlers;
                }

                _eventBus = new EventBus();
            }
            return _eventBus;
        }

        /// <summary>
        /// 
        /// </summary>
        private readonly Func<object, object, bool> eventHandlerEquals = (o1, o2) =>
        {
            var o1Type = o1.GetType();
            var o2Type = o2.GetType();
            return o1Type == o2Type;
        };

        #region �����¼�

        public void Subscribe<TEvent>(IEventHandler<TEvent> eventHandler) where TEvent : IEvent
        {
            //ͬ����
            lock (_syncObject)
            {
                //��ȡ����ģ�͵�����
                var eventType = typeof(TEvent);
                //����������������¼���������ע���
                if (_dicEventHandler.ContainsKey(eventType))
                {
                    var handlers = _dicEventHandler[eventType];
                    if (handlers != null)
                    {
                        handlers.Add(eventHandler);
                    }
                    else
                    {
                        handlers = new List<object>
                        {
                            eventHandler
                        };
                    }
                }
                else
                {
                    _dicEventHandler.Add(eventType, new List<object> { eventHandler });
                }
            }
        }

        /// <summary>
        /// �����¼�ʵ��
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subTypeList"></param>
        public void Subscribe<TEvent>(Action<TEvent> eventHandlerFunc)
            where TEvent : IEvent
        {
            Subscribe<TEvent>(new ActionDelegatedEventHandler<TEvent>(eventHandlerFunc));
        }
        public void Subscribe<TEvent>(IEnumerable<IEventHandler<TEvent>> eventHandlers)
            where TEvent : IEvent
        {
            foreach (var eventHandler in eventHandlers)
            {
                Subscribe<TEvent>(eventHandler);
            }
        }

        #endregion

        #region �����¼�

        public void Publish<TEvent>(TEvent tEvent) where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            if (_dicEventHandler.ContainsKey(eventType) && _dicEventHandler[eventType] != null &&
                _dicEventHandler[eventType].Count > 0)
            {
                var handlers = _dicEventHandler[eventType];
                try
                {
                    foreach (var handler in handlers)
                    {
                        var eventHandler = handler as IEventHandler<TEvent>;
                        eventHandler.Handle(tEvent);
                        if (onEvent!=null)
                        {
                            onEvent(tEvent,true,null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (onEvent != null)
                    {
                        onEvent(tEvent, false, ex);
                    }
                }
            }
            else
            {
                onEvent(tEvent, true, null);
            }
        }

     

        #endregion

        #region ȡ������
        /// <summary>
        /// ȡ�������¼�
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subType"></param>
        public void Unsubscribe<TEvent>(IEventHandler<TEvent> eventHandler) where TEvent : IEvent
        {
            lock (_syncObject)
            {
                var eventType = typeof(TEvent);
                if (_dicEventHandler.ContainsKey(eventType))
                {
                    var handlers = _dicEventHandler[eventType];
                    if (handlers != null
                        && handlers.Exists(deh => eventHandlerEquals(deh, eventHandler)))
                    {
                        var handlerToRemove = handlers.First(deh => eventHandlerEquals(deh, eventHandler));
                        handlers.Remove(handlerToRemove);
                    }
                }
            }
        }

        #endregion


      

    }
}
