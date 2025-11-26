package com.nexus.cross.sign.unity;

import android.content.Context;
import android.content.Intent;
import android.net.Uri;

public class Linker {
    private Linker() {
        throw new AssertionError("Utility class");
    }
    
    public static boolean canOpenURL(Context context, String url){
        Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
        return intent.resolveActivity(context.getPackageManager()) != null;
    }
}
