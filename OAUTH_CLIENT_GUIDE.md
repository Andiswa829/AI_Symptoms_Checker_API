# 🔧 OAuth Client Configuration Guide

## ✅ **Current Setup (Recommended)**

You're using **one OAuth 2.0 client** for both authentication and health data access. This is the **correct and recommended approach**.

### **Your Current Client:**
- **Name**: Symptoms Checker Web Client dev
- **Client ID**: `699206291871-pdv6amuetllvv28eplekjabbf1rsug8m.apps.googleusercontent.com`
- **Client Secret**: `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`

## 🎯 **Why One Client Works for Everything**

### **1. Scope-Based Access Control**
OAuth 2.0 uses **scopes** to control what your application can access, not separate clients.

**Authentication Scopes:**
- `profile` - Basic profile information
- `email` - Email address
- `openid` - OpenID Connect authentication

**Health Data Scopes:**
- `https://www.googleapis.com/auth/fitness.body.read`
- `https://www.googleapis.com/auth/fitness.heart_rate.read`
- `https://www.googleapis.com/auth/fitness.activity.read`
- `https://www.googleapis.com/auth/fitness.sleep.read`
- `https://www.googleapis.com/auth/fitness.blood_pressure.read`
- `https://www.googleapis.com/auth/fitness.oxygen_saturation.read`

### **2. Token Types**
From the same OAuth flow, you get:
- **ID Token**: For user authentication (JWT with user info)
- **Access Token**: For API access (includes all requested scopes)

## 🚫 **When You Might Need Separate Clients**

You would only need separate OAuth clients if:

1. **Different Applications**: Completely separate apps (web vs mobile)
2. **Different Environments**: Production vs Development vs Staging
3. **Different Organizations**: Multi-tenant scenarios
4. **Security Isolation**: Strict separation of concerns

## ✅ **Your Current Configuration is Perfect**

### **Google Cloud Console Setup:**
```
Authorized JavaScript Origins:
- http://localhost:6000
- http://localhost:7125
- https://yourdomain.com (for production)

Authorized Redirect URIs:
- http://localhost:6000/google-auth-test.html
- http://localhost:7125/oauth-debug.html
```

### **API Configuration (appsettings.json):**
```json
{
  "Google": {
    "ClientId": "699206291871-pdv6amuetllvv28eplekjabbf1rsug8m.apps.googleusercontent.com",
    "ClientSecret": "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"
  }
}
```

## 🔄 **How It Works in Practice**

### **Frontend (JavaScript):**
```javascript
// Request both authentication AND health data scopes
gapi.load('auth2', function() {
    gapi.auth2.init({
        client_id: 'YOUR_CLIENT_ID',
        scope: 'profile email https://www.googleapis.com/auth/fitness.body.read https://www.googleapis.com/auth/fitness.heart_rate.read'
    });
});
```

### **Backend (API):**
```csharp
// Same client handles both tokens
public async Task<AuthResponse> GoogleAuth(GoogleAuthRequest request)
{
    // Validate ID token for authentication
    var googleUser = await VerifyGoogleTokenAsync(request.IdToken);
    
    // Store access token for health data access
    return new AuthResponse
    {
        Token = jwtToken,
        GoogleAccessToken = request.AccessToken // Pass through for health API calls
    };
}
```

## 🚀 **Benefits of Single Client Approach**

1. **✅ Simplicity**: One set of credentials to manage
2. **✅ Cost-Effective**: No additional quotas or limits
3. **✅ User Experience**: Single consent screen for all permissions
4. **✅ Maintenance**: Easier to configure and update
5. **✅ Google's Recommendation**: This is how Google designed OAuth 2.0

## 🔍 **Troubleshooting Token Issues**

If you're getting "Invalid token" errors, it's likely due to:

1. **Missing Scopes**: Token doesn't include fitness scopes
2. **Expired Token**: Access tokens expire (usually 1 hour)
3. **Wrong Token Type**: Using ID token instead of access token for API calls
4. **Scope Mismatch**: Requested scopes don't match what's configured

## 📝 **Next Steps**

1. **Keep your current client** - it's configured correctly
2. **Ensure both tokens are passed** from frontend to backend
3. **Use access token for health API calls**
4. **Use ID token for user authentication**

Your current setup is **optimal and follows Google's best practices**! 🎉