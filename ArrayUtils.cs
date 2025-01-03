using System.Collections.Generic;

public static class ArrayUtils {
    public static bool IsSubArrayEqual(List<byte> x, List<byte> y, int start) {
        for (int i = 0; i < y.Count; i++) {
            if (x[start++] != y[i]) return false;
        }
        return true;
    }
    public static int StartingIndex(this List<byte> x, List<byte> y) {
        int max = 1 + x.Count - y.Count;
        for(int i = 0 ; i < max ; i++) {
            if(IsSubArrayEqual(x,y,i)) return i;
        }
        return -1;
    }
    // public static int RStartingIndex(this byte[] x, byte[] y) {
    // 	int max = x.Length - y.Length;
    // 	for(int i = max ; i >= 0 ; i--) {
    // 		if(IsSubArrayEqual(x,y,i)) return i;
    // 	}
    // 	return -1;
    // }
}