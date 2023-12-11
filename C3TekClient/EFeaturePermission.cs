using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient
{
    public enum EFeaturePermission
    {
        NONE = 0,
        SEND_SMS_MANUAL = 1,
        SEND_SMS_EXCEL = 2,
        SEND_SMS_WEB = 3,
        RECEIVE_SMS_MANUAL = 4,
        RECEIVE_SMS_AUTO = 5,
        RECEIVE_SMS_WEB = 6,
        SINGLE_USSD = 7,
        MULTI_PORT_SINGLE_USSD = 8,
        MULTI_PORT_SINGLE_USSD_WEB = 9, //NO NEED 
        SINGLE_MULTI_USSD = 10,
        MULTI_PORT_MULTI_USSD = 11,
        MULTI_PORT_WEB_MULTI_USSD = 12, //NO NEED
        CREATE_MY = 13,
        CALL_OUT_ONE_PHONENUMBER = 14,
        CALL_OUT_EXCEL = 15,
        CALL_OUT_WEB = 16,
        CALL_OUT_PLAY_AUDIO = 17,
        RECEIVE_AND_ACCEPT = 18,
        RECEIVE_AND_ACCEPT_RECORD = 19,
        CONSUME_DATA = 20,
        CHANGE_IMEI = 21,
        CONSUME_DATA_WEB = 22, //NO NEED
        SHOW_POPUP = 24

    }
}
