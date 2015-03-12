﻿using System;
using System.Linq;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Utilities;
using kOS.AddOns.KerbalAlarmClock;

namespace kOS.Suffixed
{
    public class KACAlarmWrapper : Structure
    {
        private KACWrapper.KACAPI.KACAlarm alarm;
        private SharedObjects shared;

        public KACAlarmWrapper(KACWrapper.KACAPI.KACAlarm init, SharedObjects shared) 
        { 
            alarm = init;
            this.shared = shared;
            InitializeSuffixes();
        }
        public KACAlarmWrapper(String alarmID, SharedObjects shared) 
        {
            alarm = KACWrapper.KAC.Alarms.First(z=>z.ID==alarmID);
            this.shared = shared;
            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("ID", new Suffix<string>(() => alarm.ID));
            AddSuffix("NAME", new SetSuffix<string>(() => alarm.Name, value => alarm.Name = value));

            //AddSuffix("VESSELID", new SetSuffix<string>(() => alarm.VesselID, value => alarm.VesselID = value));
            AddSuffix("VESSEL", new SetSuffix<VesselTarget> (() => getVesselByID (alarm.VesselID), setVesselID));

            AddSuffix("NOTES", new SetSuffix<string>(() => alarm.Name, value => alarm.Name = value));

            AddSuffix("ACTION", new SetSuffix<string> (alarm.AlarmAction.ToString, SetAlarmAction));

            AddSuffix("TYPE", new Suffix<string> (alarm.AlarmType.ToString));

            AddSuffix("REMAINING", new Suffix<double>(GetRemaining));

            AddSuffix("TIME", new SetSuffix<double>(() => alarm.AlarmTime, value => alarm.AlarmTime = value));
            AddSuffix("MARGIN", new SetSuffix<double>(() => alarm.AlarmMargin, value => alarm.AlarmMargin = value));

            AddSuffix("REPEAT", new SetSuffix<Boolean>(() => alarm.RepeatAlarm, value => alarm.RepeatAlarm = value));

            AddSuffix("REPEATPERIOD", new SetSuffix<double>(() => alarm.RepeatAlarmPeriod, value => alarm.RepeatAlarmPeriod = value));

            AddSuffix("ORIGINBODY", new SetSuffix<string>(() => alarm.XferOriginBodyName, value => alarm.XferOriginBodyName = value));
            AddSuffix("TARGETBODY", new SetSuffix<string>(() => alarm.XferTargetBodyName, value => alarm.XferTargetBodyName = value));


        }

        private VesselTarget getVesselByID (string vesselID)
        {
            if (string.IsNullOrEmpty (vesselID))
                return null;
            
            var g = new Guid(vesselID);
            Vessel v = FlightGlobals.Vessels.First (z => z.id == g);
            return v != null ? new VesselTarget (v, shared) : null;
        }

        private void setVesselID (VesselTarget v)
        {
            alarm.VesselID = v.Vessel == null ? "" : v.Vessel.id.ToString ();
        }

        private double GetRemaining()
        {
            /*SafeHouse.Logger.LogWarning (string.Format ("Trying to get remaining time, {0}", alarm.Remaining));*/
            //workaround for alarm.Remaining type mismatch
            return alarm.AlarmTime - Planetarium.GetUniversalTime();
        }
            
        private void SetAlarmAction(string newAlarmAction)
        {
            KACWrapper.KACAPI.AlarmActionEnum result;
            try
            {
                result = (KACWrapper.KACAPI.AlarmActionEnum) Enum.Parse(typeof(KACWrapper.KACAPI.AlarmActionEnum), newAlarmAction);
                alarm.AlarmAction = result;
            }
            catch (ArgumentException)
            {
                SafeHouse.Logger.LogWarning (string.Format ("Failed parsing {0} into KACAPI.AlarmActionEnum", newAlarmAction));
            }

        }
    }
}

