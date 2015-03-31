using System;

namespace AFLib
{
    public class TripData
    {
        public string startDate { get; private set; }
        public string startTime { get; private set; }
        public string endDateTime { get; private set; }
        public string tripLength { get; private set; }
        public string maxSpeed { get; private set; }
        public string endlocationlat { get; private set; }
        public string endlocationlng { get; private set; }
        public string fuelEfficiency { get; private set; }
        public string fuelLevel { get; private set; }
        public string startlocationlat { get; private set; }
        public string startlocationlng { get; private set; }
        /**
         * <summary>
         * Accepts dates of the format "dd/mm/yyyy hh:mm:ss"
         * </summary>
         */ 
        public TripData (DateTime dtStart, DateTime? dtEnd, string maxSpeed, string endlocationlat, string endlocationlng,
            string fuelEfficiency, string fuelLevel, string startlocationlat, string startlocationlng)
        {
            this.endlocationlat = endlocationlat;
            this.endlocationlng = endlocationlng;
            this.fuelEfficiency = fuelEfficiency;
            this.fuelLevel = fuelLevel;
            this.startlocationlat = startlocationlat;
            this.startlocationlng = startlocationlng;

            dtStart = dtStart.ToLocalTime ();


            DateTime? dateOrNull = dtEnd;
            //Make sure not null TODO: Make timespan human readable
            if (dateOrNull != null) {
                //get total trip length
                DateTime notNull = (DateTime)dtEnd;
                notNull = notNull.ToLocalTime ();
                //tripLength = notNull.Subtract (dtStart).ToString();
                //TimeSpan time = notNull.Subtract (dtStart);
                TimeSpan time = notNull - dtStart;
                tripLength = humanReadableTimeSpan (time);
                //string format = "MMM dd @ hh:mm tt";

                endDateTime = string.Format ("{0:MMM dd @ hh:mm tt}", dtEnd);
            }
            else {
                tripLength = "In Progress";
                endDateTime = "In Progress";
            }


            setStartDate (dtStart.Month, dtStart.Day);
            setStartTime (dtStart.Hour, dtStart.Minute);



            //set trip max speed
            string[] beforePeriod = maxSpeed.Split('.');
            this.maxSpeed = beforePeriod[0] + " km/h";


        }

        private string humanReadableTimeSpan(TimeSpan time){
            string timeSpan = "";
            if(time.Hours != 0){
                timeSpan += time.Hours + " hrs, ";
            }
            if(time.Minutes != 0){
                timeSpan += time.Minutes + " min";
            } else {
                timeSpan = time.Seconds + " sec";
            }
            return timeSpan; 
        }

        private void setStartDate(int month, int day){

            switch(month){
            case 1:
                startDate = "Jan " + day;
                break;
            case 2:
                startDate = "Feb " + day;
                break;
            case 3:
                startDate = "Mar " + day;
                break;
            case 4: 
                startDate = "Apr " + day;
                break;
            case 5:
                startDate = "May " + day;
                break;
            case 6: 
                startDate = "Jun " + day;
                break;
            case 7: 
                startDate = "Jul " + day;
                break;
            case 8: 
                startDate = "Aug " + day;
                break;
            case 9: 
                startDate = "Sep " + day;
                break;
            case 10:
                startDate = "Oct " + day;
                break;
            case 11: 
                startDate = "Nov " + day;
                break;
            case 12: 
                startDate = "Dec " + day;
                break;
            default: 
                startDate = "Jan"  + day;
                break;
            }
        }

        private void setStartTime(int hour, int min){
            String minString = min.ToString();
            if(min < 10){
                minString = "0" + min;
            }

            if( hour == 0){
                hour += 12;
                startTime = hour + ":" + minString + "am";
            }
            else if(hour < 13){
                startTime = hour + ":" + minString + "am";
            }
            else if(hour > 12){
                hour -= 12;
                startTime = hour + ":" + minString + "pm";
            }
        }
    }
}
