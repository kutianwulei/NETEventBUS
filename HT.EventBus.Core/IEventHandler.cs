/****************************************************************
** �ļ���:   IEventHandler.cs
** ��Ҫ��:   IEventHandler  
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

namespace HT.EventBus.Core
{
    /// <summary>
    /// �¼�����ӿ�
    /// </summary>
    /// <typeparam name="TEvent">�̳�IEvent������¼�Դ����</typeparam>
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// �������
        /// </summary>
        /// <param name="evt"></param>
        void Handle(TEvent evt);

    }
}