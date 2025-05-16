// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("1cCY+61kle7/q1BAaPy5IVohGZCqYfihSMgsRH1S0dO//4AMiKliX4eRqnQjmZhfke+RM79LTI47Suf8aLGGSy11IqFiskC+cFLFFN0UVLREc1XBvVRyQaKP1NpecC7eNEELnNcsVIXcuTTCnMHf3nWitlTualYVHp2TnKwenZaeHp2dnCtIJCKOfT4hQ9RV8g2VdSu5b/LceIXYxQni5EmfzeVU7bxyNHvjJqo9dKF/GUMsR10vSWYDoAzjphZF0NXr6lrBYdesHp2+rJGalbYa1BprkZ2dnZmcn3N2gPheVmbKxi5czzm94Hg8/l8/2MKJweWTA41kUM8KXFh3FmI5yE/qkYJ9rTdFvrkhEK7mwEdFqFPzGoQTNZCn+g0xx56fnZyd");
        private static int[] order = new int[] { 5,4,3,3,12,10,12,8,13,12,10,11,12,13,14 };
        private static int key = 156;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
