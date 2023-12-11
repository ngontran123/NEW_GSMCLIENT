using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace C3TekClient.C3Tek
{
    public static class TTTBHelper
    {
        private static List<TTTBPusher> Sequences = new List<TTTBPusher>();
        private static object lockSequence = new object();
        public static void RequestPush(string phone)
        {
            lock (lockSequence)
            {
                var sequence = Sequences.FirstOrDefault(sq => sq.Phone == phone);
                if (sequence == null)
                {
                    Sequences.Add(new TTTBPusher()
                    {
                        Phone = phone,
                        Pushed = false,
                        TTTB = null,
                        MyCompleted = true
                    }) ;
                }
                else
                {
                    if (sequence.Pushed)
                        return;
                    if (sequence.TTTB == null)
                        return;
                    sequence.MyCompleted = true;
                }
            }
        }
        public static void RequestPush(TTTB tttb)
        {
            lock (lockSequence)
            {
                var sequence = Sequences.FirstOrDefault(sq => sq.Phone == tttb.phone);
                if (sequence == null)
                {
                    Sequences.Add(new TTTBPusher()
                    {
                        Phone = tttb.phone,
                        Pushed = false,
                        TTTB = new TTTB()
                        {
                            address_cmnd = tttb.address_cmnd,
                            birthday = tttb.birthday,
                            cmnd = tttb.cmnd,
                            date_active = tttb.date_active,
                            date_cmnd = tttb.date_cmnd,
                            full_content = tttb.full_content,
                            full_name = tttb.full_name,
                            phone = tttb.phone,
                            type_tb = tttb.type_tb
                        },
                        GSMCompleted = true
                    });
                }
                else
                {
                    if (sequence.Pushed)
                        return;
                    sequence.TTTB = new TTTB()
                    {
                        address_cmnd = tttb.address_cmnd,
                        birthday = tttb.birthday,
                        cmnd = tttb.cmnd,
                        date_active = tttb.date_active,
                        date_cmnd = tttb.date_cmnd,
                        full_content = tttb.full_content,
                        full_name = tttb.full_name,
                        phone = tttb.phone,
                        type_tb = tttb.type_tb
                    };
                    sequence.GSMCompleted = true;
                }
            }
        }
        public static void Start()
        {
            new Thread(new ThreadStart(() =>
            {
                while (!GlobalVar.IsApplicationExit)
                {
                    try
                    {
                        List<TTTBPusher> sequences = new List<TTTBPusher>();
                        lock (lockSequence)
                        {
                            sequences = Sequences.Where(sq => sq.AllowPush && !sq.Pushed).ToList();
                        }
                        if (sequences.Any())
                        {
                            var portal = new C3TekPortal();
                            foreach (var sequence in sequences)
                            {
                                portal.UpdateTTTB(sequence.TTTB);
                                sequence.Pushed = true;
                            }
                        }
                    }
                    catch { }
                    Thread.Sleep(3000);
                }
            })).Start();
        }
    }

    public class TTTBPusher
    {
        public string Phone { get; set; }
        public TTTB TTTB { get; set; }
        public bool AllowPush { get { return GSMCompleted && MyCompleted; } }
        public bool Pushed { get; set; }
        public bool GSMCompleted { get; set; }
        public bool MyCompleted { get; set; }
    }
}
