namespace STC.Speech.aliyun
{
    internal class SpeechSynthesizerRESTful
    {
        public SpeechSynthesizerRESTful(string appkey, string token)
        {
            this.appkey = appkey;
            this.token = token;
        }

        public string appkey { get; set; }
        public string token { get; set; }
    }
}