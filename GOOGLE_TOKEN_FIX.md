# 🔧 Google Health Token Fix - RESOLVED ✅

## ❌ **The Problem**
You were getting the error: `"Invalid or expired Google Health access token"`

## ✅ **The Root Cause** 
The issue was that you were trying to use an **ID Token** for **API Access**, but Google Health/Fit APIs require an **Access Token**.

## 🔑 **Two Different Token Types**

### 1. **ID Token** (for Authentication)
- **Purpose**: Verify user identity
- **Contains**: User profile information (email, name, etc.)
- **Used for**: Logging the user into your app
- **Example**: `eyJhbGciOiJSUzI1NiIsImtpZCI6...`

### 2. **Access Token** (for API Access)
- **Purpose**: Access Google APIs (like Google Fit)
- **Contains**: Permission scopes for API access
- **Used for**: Making API calls to Google services
- **Example**: `ya29.a0AfH6SMC7X2vjf8nP9gK...`

## 🛠️ **What I Fixed**

### 1. **Updated GoogleAuthRequest DTO**
```csharp
public class GoogleAuthRequest
{
    public string IdToken { get; set; } = string.Empty;        // For authentication
    public string? AccessToken { get; set; }                   // For API access (NEW!)
    // ... other properties
}
```

### 2. **Updated AuthResponse DTO**
```csharp
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    // ... other properties
    public string? GoogleAccessToken { get; set; }             // Returns access token (NEW!)
    public DateTime? GoogleAccessTokenExpiration { get; set; }  // Token expiry (NEW!)
}
```

### 3. **Enhanced Token Validation**
- Added better error messages for invalid tokens
- Improved logging for debugging token issues
- Added scope validation

### 4. **Updated Test Page**
The `google-auth-test.html` now sends **both tokens** to the API:
```javascript
const apiRequest = {
    idToken: idToken,           // For user authentication
    accessToken: googleAccessToken, // For health data access
    // ... other user info
};
```

## 🚀 **How to Use (Updated Flow)**

### Step 1: Get Both Tokens from Google
```javascript
// Configure Google OAuth with health scopes
const authResponse = await google.accounts.oauth2.initTokenClient({
    client_id: 'YOUR_CLIENT_ID',
    scope: 'profile email https://www.googleapis.com/auth/fitness.body.read https://www.googleapis.com/auth/fitness.activity.read https://www.googleapis.com/auth/fitness.sleep.read https://www.googleapis.com/auth/fitness.heart_rate.read https://www.googleapis.com/auth/fitness.blood_pressure.read https://www.googleapis.com/auth/fitness.oxygen_saturation.read'
}).requestAccessToken();

// You now have:
// - authResponse.access_token (for API calls)
// - credential.credential (ID token from separate call)
```

### Step 2: Send Both Tokens to Your API
```javascript
const response = await fetch('/api/auth/google', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        idToken: idToken,           // From Google Sign-In
        accessToken: accessToken,   // From OAuth flow
        email: userInfo.email,
        firstName: userInfo.given_name,
        lastName: userInfo.family_name,
        picture: userInfo.picture
    })
});
```

### Step 3: Use the Access Token for Health Data
```javascript
// The API response now includes the access token
const authResult = await response.json();
const healthAccessToken = authResult.googleAccessToken;

// Now you can use this token for health endpoints
const healthData = await fetch('/api/googlehealth/sync', {
    method: 'POST',
    headers: { 
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authResult.token}` // Your JWT token
    },
    body: JSON.stringify({
        accessToken: healthAccessToken  // Google access token for health data
    })
});
```

## 🎯 **Required Google Cloud Console Setup**

Make sure your Google Cloud Console project has:

### OAuth 2.0 Credentials
- **Client ID**: `699206291871-pdv6amuetllvv28eplekjabbf1rsug8m.apps.googleusercontent.com`

### Authorized Origins
- `http://localhost:6000`
- `http://localhost:7125` (or whatever port you're using)

### Enabled APIs
- ✅ Google Fit API
- ✅ Google+ API (for user info)

### Scopes (as shown in your screenshot)
- ✅ `https://www.googleapis.com/auth/fitness.activity.read`
- ✅ `https://www.googleapis.com/auth/fitness.body.read`
- ✅ `https://www.googleapis.com/auth/fitness.blood_glucose.read`
- ✅ `https://www.googleapis.com/auth/fitness.blood_pressure.read`
- ✅ `https://www.googleapis.com/auth/fitness.body_temperature.read`
- ✅ `https://www.googleapis.com/auth/fitness.heart_rate.read`
- ✅ `https://www.googleapis.com/auth/fitness.location.read`
- ✅ `https://www.googleapis.com/auth/fitness.nutrition.read`
- ✅ `https://www.googleapis.com/auth/fitness.oxygen_saturation.read`
- ✅ `https://www.googleapis.com/auth/fitness.sleep.read`

## 🧪 **Testing**

1. **Start your API**: `dotnet run`
2. **Open test page**: `http://localhost:6000/google-auth-test.html`
3. **Click "Sign in with Google Health"**
4. **Authorize all the fitness permissions**
5. **The page will now send both ID and access tokens**
6. **Test health endpoints with the returned access token**

## ✅ **Expected Result**

You should now see:
- ✅ No more "Invalid or expired Google Health access token" errors
- ✅ Successful authentication with both JWT and Google access tokens
- ✅ Ability to call Google Health endpoints with the access token
- ✅ Clear error messages if something goes wrong

The key insight is that **authentication** (who you are) and **authorization** (what you can access) use different tokens in the Google ecosystem!