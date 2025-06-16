namespace ConfigurationConstants
{
    public class Constants
    {
        #region "Properties&Attributes"
        #region "Region Names"
        public static string MessageRegion = "MessageRegion";
        #endregion "Region Names"


        /// <summary>
        /// this is the port that the message service will transmit on and
        /// clients will have to listen to
        /// To be more dynaic the use of a general confiruation file should
        /// be used to make the configuration more dynamic
        /// </summary>
        public static int MessageServicePort = 5001;

        #endregion "Properties&Attributes"
    }
}