# 🌐 Frontend Google OAuth Integration for Health Data

## 📋 **Frontend Setup for Google Health Data**

Your backend is already configured! Now you need to set up the frontend to get **real Google OAuth tokens** with fitness scopes.

## 🔧 **Method 1: HTML + JavaScript (Quick Testing)**

Create this test page to get Google tokens:

### **Create Test Page**
```html
<!DOCTYPE html>
<html>
<head>
    <title>Google Health OAuth Test</title>
    <script src="https://apis.google.com/js/api.js"></script>
    <script src="https://accounts.google.com/gsi/client" async defer></script>
</head>
<body>
    <h1>🏥 Google Health OAuth Test</h1>
    
    <div id="signin-div"></div>
    <button onclick="signOut()">Sign Out</button>
    
    <div id="token-info" style="margin-top: 20px; display: none;">
        <h3>✅ OAuth Tokens</h3>
        <div>
            <label>Google Access Token (for Health API):</label><br>
            <textarea id="access-token" rows="3" cols="80" readonly></textarea>
            <button onclick="copyToClipboard('access-token')">Copy</button>
        </div>
        <br>
        <div>
            <label>Google ID Token (for Authentication):</label><br>
            <textarea id="id-token" rows="3" cols="80" readonly></textarea>
            <button onclick="copyToClipboard('id-token')">Copy</button>
        </div>
        <br>
        <button onclick="testHealthAPI()">🧪 Test Health API</button>
        <div id="health-results"></div>
    </div>

    <script>
        let googleAuth;
        let currentUser;
        
        // Required scopes for Google Health/Fit data
        const HEALTH_SCOPES = [
            'https://www.googleapis.com/auth/fitness.body.read',
            'https://www.googleapis.com/auth/fitness.activity.read',
            'https://www.googleapis.com/auth/fitness.sleep.read',
            'https://www.googleapis.com/auth/fitness.heart_rate.read',
            'https://www.googleapis.com/auth/fitness.blood_pressure.read',
            'https://www.googleapis.com/auth/fitness.oxygen_saturation.read'
        ].join(' ');

        // Initialize Google API
        function initGoogleAuth() {
            gapi.load('auth2', function() {
                googleAuth = gapi.auth2.init({
                    client_id: '699206291871-pdv6amuetllvv28eplekjabbf1rsug8m.apps.googleusercontent.com',
                    scope: HEALTH_SCOPES
                });
                
                // Render sign-in button
                googleAuth.attachClickHandler(
                    document.getElementById('signin-div'),
                    {},
                    onSignIn,
                    onSignInError
                );
                
                // Create sign-in button
                const signInButton = document.createElement('button');
                signInButton.textContent = '🔑 Sign in with Google Health';
                signInButton.onclick = signInWithHealth;
                document.getElementById('signin-div').appendChild(signInButton);
            });
        }

        // Sign in with health scopes
        async function signInWithHealth() {
            try {
                const user = await googleAuth.signIn({
                    scope: HEALTH_SCOPES
                });
                onSignIn(user);
            } catch (error) {
                console.error('Sign-in error:', error);
                alert('Sign-in failed: ' + error.error);
            }
        }

        // Handle successful sign-in
        function onSignIn(googleUser) {
            currentUser = googleUser;
            const authResponse = googleUser.getAuthResponse();
            const profile = googleUser.getBasicProfile();
            
            console.log('✅ Google Sign-In Successful!');
            console.log('Access Token:', authResponse.access_token);
            console.log('ID Token:', authResponse.id_token);
            
            // Display tokens
            document.getElementById('access-token').value = authResponse.access_token;
            document.getElementById('id-token').value = authResponse.id_token;
            document.getElementById('token-info').style.display = 'block';
            
            // Log user info
            console.log('User Info:', {
                name: profile.getName(),
                email: profile.getEmail(),
                picture: profile.getImageUrl()
            });
            
            alert('✅ Successfully signed in with Google Health scopes!\\nCheck the tokens below.');
        }

        // Handle sign-in error
        function onSignInError(error) {
            console.error('Sign-in error:', error);
            alert('❌ Sign-in failed. Make sure you grant all health permissions.');
        }

        // Sign out
        function signOut() {
            if (googleAuth) {
                googleAuth.signOut().then(() => {
                    document.getElementById('token-info').style.display = 'none';
                    document.getElementById('health-results').innerHTML = '';
                    console.log('User signed out');
                });
            }
        }

        // Copy token to clipboard
        function copyToClipboard(elementId) {
            const element = document.getElementById(elementId);
            element.select();
            document.execCommand('copy');
            alert('✅ Token copied to clipboard!');
        }

        // Test your Health API
        async function testHealthAPI() {
            if (!currentUser) {
                alert('Please sign in first');
                return;
            }
            
            const authResponse = currentUser.getAuthResponse();
            const accessToken = authResponse.access_token;
            const idToken = authResponse.id_token;
            
            try {
                // First, authenticate with your API
                const authResult = await fetch('http://localhost:3000/api/auth/google', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        idToken: idToken
                    })
                });
                
                const authData = await authResult.json();
                console.log('API Auth Response:', authData);
                
                if (!authResult.ok) {
                    throw new Error(authData.message || 'Authentication failed');
                }
                
                const jwtToken = authData.token;
                
                // Test health token validation
                const validateResult = await fetch('http://localhost:3000/api/googlehealth/validate-token', {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${jwtToken}`,
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(accessToken)
                });
                
                const validateData = await validateResult.json();
                console.log('Token Validation:', validateData);
                
                // Test heart rate data
                const heartRateResult = await fetch(`http://localhost:3000/api/googlehealth/heart-rate?accessToken=${accessToken}`, {
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${jwtToken}`
                    }
                });
                
                const heartRateData = await heartRateResult.json();
                console.log('Heart Rate Data:', heartRateData);
                
                // Display results
                document.getElementById('health-results').innerHTML = `
                    <h4>🧪 API Test Results</h4>
                    <div style="background: #f5f5f5; padding: 10px; margin: 10px 0;">
                        <strong>✅ Authentication:</strong> ${authResult.ok ? 'Success' : 'Failed'}<br>
                        <strong>✅ Token Validation:</strong> ${validateData.valid ? 'Valid' : 'Invalid'}<br>
                        <strong>📊 Heart Rate Data:</strong> ${heartRateResult.ok ? 'Available' : 'No data/Permission needed'}<br>
                    </div>
                    <details>
                        <summary>📋 Full Response Details</summary>
                        <pre style="background: #f0f0f0; padding: 10px; overflow: auto;">
Authentication: ${JSON.stringify(authData, null, 2)}

Token Validation: ${JSON.stringify(validateData, null, 2)}

Heart Rate: ${JSON.stringify(heartRateData, null, 2)}
                        </pre>
                    </details>
                `;
                
            } catch (error) {
                console.error('API Test Error:', error);
                document.getElementById('health-results').innerHTML = `
                    <div style="background: #ffebee; padding: 10px; margin: 10px 0; color: #c62828;">
                        <strong>❌ Error:</strong> ${error.message}
                    </div>
                `;
            }
        }

        // Initialize when page loads
        window.onload = function() {
            initGoogleAuth();
        };
    </script>
</body>
</html>
```

### **How to Use This Test Page:**

1. **Save as** `google-health-test.html` in your `wwwroot` folder
2. **Open** http://localhost:3000/google-health-test.html
3. **Click "Sign in with Google Health"**
4. **Grant all health permissions** when prompted
5. **Copy the tokens** and use them in your API tests
6. **Click "Test Health API"** to see live integration

## 🔧 **Method 2: React Integration (Production)**

### **Install Dependencies**
```bash
npm install google-auth-library
# or
npm install @google-cloud/local-auth googleapis
```

### **React Component Example**
```jsx
import React, { useState, useEffect } from 'react';

const GoogleHealthAuth = () => {
    const [isSignedIn, setIsSignedIn] = useState(false);
    const [tokens, setTokens] = useState({ accessToken: '', idToken: '' });
    const [healthData, setHealthData] = useState(null);
    
    const HEALTH_SCOPES = [
        'https://www.googleapis.com/auth/fitness.body.read',
        'https://www.googleapis.com/auth/fitness.activity.read',
        'https://www.googleapis.com/auth/fitness.sleep.read',
        'https://www.googleapis.com/auth/fitness.heart_rate.read',
        'https://www.googleapis.com/auth/fitness.blood_pressure.read',
        'https://www.googleapis.com/auth/fitness.oxygen_saturation.read'
    ].join(' ');

    useEffect(() => {
        // Load Google API
        const script = document.createElement('script');
        script.src = 'https://apis.google.com/js/api.js';
        script.onload = initGoogleAuth;
        document.head.appendChild(script);
    }, []);

    const initGoogleAuth = () => {
        window.gapi.load('auth2', () => {
            const authInstance = window.gapi.auth2.init({
                client_id: '699206291871-pdv6amuetllvv28eplekjabbf1rsug8m.apps.googleusercontent.com',
                scope: HEALTH_SCOPES
            });
            
            // Check if already signed in
            if (authInstance.isSignedIn.get()) {
                handleSignIn(authInstance.currentUser.get());
            }
        });
    };

    const signIn = async () => {
        try {
            const authInstance = window.gapi.auth2.getAuthInstance();
            const user = await authInstance.signIn({
                scope: HEALTH_SCOPES
            });
            handleSignIn(user);
        } catch (error) {
            console.error('Sign-in failed:', error);
        }
    };

    const handleSignIn = (user) => {
        const authResponse = user.getAuthResponse();
        setTokens({
            accessToken: authResponse.access_token,
            idToken: authResponse.id_token
        });
        setIsSignedIn(true);
    };

    const syncHealthData = async () => {
        try {
            // Authenticate with your API first
            const authResponse = await fetch('/api/auth/google', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ idToken: tokens.idToken })
            });
            
            const { token: jwtToken } = await authResponse.json();
            
            // Sync health data
            const healthResponse = await fetch('/api/googlehealth/sync', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${jwtToken}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    accessToken: tokens.accessToken,
                    dataTypes: ['heart_rate', 'blood_pressure', 'sleep', 'activity']
                })
            });
            
            const healthData = await healthResponse.json();
            setHealthData(healthData);
        } catch (error) {
            console.error('Health sync failed:', error);
        }
    };

    const signOut = () => {
        const authInstance = window.gapi.auth2.getAuthInstance();
        authInstance.signOut();
        setIsSignedIn(false);
        setTokens({ accessToken: '', idToken: '' });
        setHealthData(null);
    };

    return (
        <div className="google-health-auth">
            {!isSignedIn ? (
                <button onClick={signIn} className="btn btn-primary">
                    🔑 Connect Google Health
                </button>
            ) : (
                <div>
                    <div className="user-info">
                        ✅ Connected to Google Health
                        <button onClick={signOut} className="btn btn-secondary ml-2">
                            Sign Out
                        </button>
                    </div>
                    
                    <button onClick={syncHealthData} className="btn btn-success mt-2">
                        📊 Sync Health Data
                    </button>
                    
                    {healthData && (
                        <div className="health-dashboard mt-3">
                            <h4>📈 Your Health Data</h4>
                            {healthData.heartRate && (
                                <div className="metric">
                                    ❤️ Heart Rate: {healthData.heartRate.currentHeartRate} BPM
                                </div>
                            )}
                            {healthData.sleep && (
                                <div className="metric">
                                    😴 Sleep: {Math.floor(healthData.sleep.totalSleepTime / 60)}h {healthData.sleep.totalSleepTime % 60}m
                                </div>
                            )}
                            {/* Add more health metrics as needed */}
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};

export default GoogleHealthAuth;
```

## 🔧 **Method 3: OAuth Playground (Quick Token Generation)**

### **For Immediate Testing:**

1. **Go to**: [Google OAuth 2.0 Playground](https://developers.google.com/oauthplayground/)

2. **Configure Settings** (click gear icon):
   - ✅ Use your own OAuth credentials
   - **OAuth Client ID**: `699206291871-pdv6amuetllvv28eplekjabbf1rsug8m.apps.googleusercontent.com`
   - **OAuth Client secret**: `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`

3. **Select Scopes** (Step 1):
   ```
   https://www.googleapis.com/auth/fitness.body.read
   https://www.googleapis.com/auth/fitness.activity.read
   https://www.googleapis.com/auth/fitness.sleep.read
   https://www.googleapis.com/auth/fitness.heart_rate.read
   https://www.googleapis.com/auth/fitness.blood_pressure.read
   https://www.googleapis.com/auth/fitness.oxygen_saturation.read
   ```

4. **Authorize APIs** → **Exchange authorization code for tokens**

5. **Copy Access Token** and use in your API tests!

## ⚠️ **Important Requirements**

### **For Real Health Data:**
- ✅ Google account with **Google Fit app** installed
- ✅ **Active health/fitness data** (walk around, sleep with phone/watch)
- ✅ **Wearable device** (optional but recommended):
  - Fitbit (synced to Google Fit)
  - Samsung Health (synced to Google Fit)
  - Wear OS watch
  - Any device that syncs to Google Fit

### **Health Data Sources:**
- 📱 **Smartphone sensors** (steps, activity)
- ⌚ **Smartwatch/fitness tracker** (heart rate, sleep)
- 🏥 **Manual entries** in Google Fit app
- 🔗 **Connected apps** (MyFitnessPal, Strava, etc.)

## 🧪 **Testing Steps**

1. **Create the test HTML page** (Method 1)
2. **Sign in with Google Health permissions**
3. **Copy the access tokens**
4. **Use tokens in your API tests** (`SymptomsCheckerApi-GoogleHealth.http`)
5. **Verify health data retrieval**

## 🎯 **Expected Results**

### **With Real Health Data:**
```json
{
  "heartRate": {
    "currentHeartRate": 72,
    "restingHeartRate": 65,
    "readings": [...]
  },
  "sleep": {
    "totalSleepTime": 450,
    "sleepEfficiency": 0.85
  },
  "activity": {
    "steps": 8542,
    "distance": 6.2
  }
}
```

### **Without Health Data:**
```json
{
  "message": "No heart rate data found for the specified period"
}
```

Your backend is **fully ready** - you just need to get the Google OAuth tokens with the right scopes! 🚀