using System;

namespace BTV.Data
{
    public class BtvEvent : EegEvent
    {
        public int Duration { get; set; } = 0;
        public string SiteOfInterest { get; set; } = "";
        public string SecondSiteOfInterest { get; set; } = "";
        public string Comment { get; set; } = "";
        public float[] Correlation { get; set; } = null;
        public float[][] Correlation2D { get; set; } = null;

        /// <summary>
        /// Constructor
        /// <param name="code">Code of the event</param>
        /// <param name="time">Moment in time of the event (in milliseconds)</param>
        /// <param name="duration">Duration of the event (in milliseconds) </param>
        /// <param name="elecOfInterest">Site of interest when registering the event </param>
        /// <param name="secondElecOfInterest">Second Site of interest when registering the event </param>
        /// <param name="comment">Comment describing the event </param>
        /// </summary>
        public BtvEvent(int code, float time, int duration = 0, string elecOfInterest = "", string secondElecOfInterest = "", string comment = "") : base(code, time)
        {
            Duration = duration;
            SiteOfInterest = elecOfInterest;
            SecondSiteOfInterest = secondElecOfInterest;
            Comment = comment;
        }

        /// <summary>
        /// Copy Constructor
        /// <param name="btvEvent">Event to copy</param>
        /// </summary>
        public BtvEvent(BtvEvent btvEvent) : base(btvEvent.Code, btvEvent.TimeInMilliSeconds)
        {
            Duration = btvEvent.Duration;
            SiteOfInterest = btvEvent.SiteOfInterest;
            SecondSiteOfInterest = btvEvent.SecondSiteOfInterest;
            Comment = btvEvent.Comment;

            if (btvEvent.Correlation != null)
                Array.Copy(btvEvent.Correlation, Correlation, Correlation.Length);
        }
    }
}
