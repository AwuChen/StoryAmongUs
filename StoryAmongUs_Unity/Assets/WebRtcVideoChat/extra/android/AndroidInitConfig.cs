
namespace Byn.Awrtc.Unity
{
    public class AndroidInitConfig
    {
        public bool hardwareAcceleration = false;
        public bool useTextures = false;
        public string preferredCodec = null;
        public bool forcePreferredCodec = false;

        public override string ToString()
        {
            return "{hardwareAcceleration:" + hardwareAcceleration + ", useTextures:" + useTextures
                + ", preferredCodec:" + preferredCodec + ", forcePreferredCodec:" + forcePreferredCodec + "}";
        }
    }
}
