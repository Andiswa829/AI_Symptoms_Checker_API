# 🔗 React Frontend Integration Guide

This guide shows how to integrate Google Sign-In with health data access from your React frontend with the updated backend API.

## 🚀 Frontend Setup

### 1. Install Dependencies

```bash
npm install @google-cloud/local-auth googleapis
# or
yarn add @google-cloud/local-auth googleapis
```

### 2. Google OAuth Configuration

```typescript
// src/config/google.ts
export const GOOGLE_CONFIG = {
  clientId: '699206291871-pdv6amuetllvv28eplekjabbf1rsug8m.apps.googleusercontent.com',
  scopes: [
    'profile',
    'email',
    'https://www.googleapis.com/auth/fitness.body.read',
    'https://www.googleapis.com/auth/fitness.activity.read',
    'https://www.googleapis.com/auth/fitness.sleep.read',
    'https://www.googleapis.com/auth/fitness.heart_rate.read',
    'https://www.googleapis.com/auth/fitness.blood_pressure.read',
    'https://www.googleapis.com/auth/fitness.oxygen_saturation.read'
  ]
};
```

### 3. Google Auth Service

```typescript
// src/services/googleAuthService.ts
import { GOOGLE_CONFIG } from '../config/google';

export interface GoogleAuthResult {
  idToken: string;
  accessToken: string;
  userInfo: {
    email: string;
    name: string;
    given_name: string;
    family_name: string;
    picture: string;
  };
}

export class GoogleAuthService {
  private gapi: any;
  private auth2: any;
  
  async initializeGoogleAuth(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (typeof window === 'undefined') {
        reject(new Error('Google Auth can only be used in browser'));
        return;
      }

      // Load Google API script
      const script = document.createElement('script');
      script.src = 'https://apis.google.com/js/api.js';
      script.onload = () => {
        window.gapi.load('auth2', () => {
          this.gapi = window.gapi;
          this.auth2 = this.gapi.auth2.init({
            client_id: GOOGLE_CONFIG.clientId,
            scope: GOOGLE_CONFIG.scopes.join(' ')
          });
          resolve();
        });
      };
      script.onerror = () => reject(new Error('Failed to load Google API'));
      document.head.appendChild(script);
    });
  }

  async signInWithGoogle(): Promise<GoogleAuthResult> {
    if (!this.auth2) {
      await this.initializeGoogleAuth();
    }

    try {
      // Get ID token for authentication
      const authResult = await this.auth2.signIn();
      const authResponse = authResult.getAuthResponse();
      const profile = authResult.getBasicProfile();

      // Get access token for API access (with health scopes)
      const accessTokenResult = await new Promise<any>((resolve, reject) => {
        this.gapi.load('auth2', () => {
          const authInstance = this.gapi.auth2.getAuthInstance();
          authInstance.grantOfflineAccess({
            scope: GOOGLE_CONFIG.scopes.join(' ')
          }).then(resolve).catch(reject);
        });
      });

      return {
        idToken: authResponse.id_token,
        accessToken: accessTokenResult.access_token || authResponse.access_token,
        userInfo: {
          email: profile.getEmail(),
          name: profile.getName(),
          given_name: profile.getGivenName(),
          family_name: profile.getFamilyName(),
          picture: profile.getImageUrl()
        }
      };
    } catch (error) {
      throw new Error(\`Google sign-in failed: \${error}\`);
    }
  }

  async signOut(): Promise<void> {
    if (this.auth2) {
      await this.auth2.signOut();
    }
  }
}
```

### 4. API Service

```typescript
// src/services/authService.ts
const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:6000';

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiration: string;
  googleAccessToken?: string;
  googleAccessTokenExpiration?: string;
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    isExternalAuth: boolean;
    profilePicture?: string;
  };
  isNewUser: boolean;
  externalProvider?: string;
}

export interface ExternalAuthRequest {
  provider: string;
  idToken: string;
  accessToken?: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  picture?: string;
}

export class AuthService {
  async authenticateWithGoogle(googleAuth: GoogleAuthResult): Promise<AuthResponse> {
    const request: ExternalAuthRequest = {
      provider: 'google',
      idToken: googleAuth.idToken,
      accessToken: googleAuth.accessToken, // Include access token for health data
      email: googleAuth.userInfo.email,
      firstName: googleAuth.userInfo.given_name,
      lastName: googleAuth.userInfo.family_name,
      picture: googleAuth.userInfo.picture
    };

    const response = await fetch(\`\${API_BASE_URL}/api/auth/external\`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Authentication failed');
    }

    return await response.json();
  }

  async getHealthData(jwtToken: string, googleAccessToken: string) {
    const response = await fetch(\`\${API_BASE_URL}/api/googlehealth/sync\`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': \`Bearer \${jwtToken}\`
      },
      body: JSON.stringify({
        accessToken: googleAccessToken
      })
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to fetch health data');
    }

    return await response.json();
  }
}
```

### 5. React Component Example

```typescript
// src/components/LoginScreen.tsx
import React, { useState, useEffect } from 'react';
import { GoogleAuthService, GoogleAuthResult } from '../services/googleAuthService';
import { AuthService, AuthResponse } from '../services/authService';

export const LoginScreen: React.FC = () => {
  const [googleAuthService] = useState(new GoogleAuthService());
  const [authService] = useState(new AuthService());
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [user, setUser] = useState<AuthResponse | null>(null);

  useEffect(() => {
    // Initialize Google Auth when component mounts
    googleAuthService.initializeGoogleAuth().catch(err => {
      setError('Failed to initialize Google Auth');
      console.error(err);
    });
  }, [googleAuthService]);

  const handleGoogleSignIn = async () => {
    setIsLoading(true);
    setError(null);

    try {
      // Step 1: Get Google tokens and user info
      const googleAuth: GoogleAuthResult = await googleAuthService.signInWithGoogle();
      
      console.log('Google Auth successful:', googleAuth);

      // Step 2: Authenticate with your backend API
      const authResponse: AuthResponse = await authService.authenticateWithGoogle(googleAuth);
      
      console.log('Backend auth successful:', authResponse);

      // Step 3: Store user and tokens
      setUser(authResponse);
      
      // Store tokens in localStorage or state management
      localStorage.setItem('jwtToken', authResponse.token);
      localStorage.setItem('googleAccessToken', authResponse.googleAccessToken || '');

      // Step 4: Optionally fetch health data immediately
      if (authResponse.googleAccessToken) {
        try {
          const healthData = await authService.getHealthData(
            authResponse.token, 
            authResponse.googleAccessToken
          );
          console.log('Health data:', healthData);
        } catch (healthError) {
          console.warn('Could not fetch health data:', healthError);
        }
      }

    } catch (err: any) {
      setError(err.message || 'Sign in failed');
      console.error('Sign in error:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSignOut = async () => {
    try {
      await googleAuthService.signOut();
      setUser(null);
      localStorage.removeItem('jwtToken');
      localStorage.removeItem('googleAccessToken');
    } catch (err) {
      console.error('Sign out error:', err);
    }
  };

  if (user) {
    return (
      <div className="user-dashboard">
        <h2>Welcome, {user.user.firstName}!</h2>
        <img src={user.user.profilePicture} alt="Profile" />
        <p>Email: {user.user.email}</p>
        <p>Health Access: {user.googleAccessToken ? '✅ Enabled' : '❌ Disabled'}</p>
        <button onClick={handleSignOut}>Sign Out</button>
      </div>
    );
  }

  return (
    <div className="login-screen">
      <h1>Symptoms Checker</h1>
      
      {error && (
        <div className="error-message" style={{ color: 'red' }}>
          {error}
        </div>
      )}

      <button 
        onClick={handleGoogleSignIn}
        disabled={isLoading}
        className="google-signin-button"
      >
        {isLoading ? 'Signing in...' : '🔗 Sign in with Google Health'}
      </button>

      <div className="scopes-info">
        <p>This app will request access to:</p>
        <ul>
          <li>Your basic profile information</li>
          <li>Your Google Fit health data (optional)</li>
        </ul>
      </div>
    </div>
  );
};
```

## 🔗 API Endpoints

### Authentication
```
POST /api/auth/external
```

**Request:**
```json
{
  "provider": "google",
  "idToken": "eyJhbGciOiJSUzI1NiIs...",
  "accessToken": "ya29.a0AfH6SMC7X2vjf8nP9gK...",
  "email": "user@gmail.com",
  "firstName": "John",
  "lastName": "Doe",
  "picture": "https://lh3.googleusercontent.com/..."
}
```

**Response:**
```json
{
  "token": "your-jwt-token",
  "refreshToken": "refresh-token",
  "googleAccessToken": "ya29.a0AfH6SMC7X2vjf8nP9gK...",
  "googleAccessTokenExpiration": "2025-11-09T12:00:00Z",
  "user": {
    "id": "user-id",
    "email": "user@gmail.com",
    "firstName": "John",
    "lastName": "Doe",
    "isExternalAuth": true,
    "profilePicture": "https://lh3.googleusercontent.com/..."
  },
  "isNewUser": false,
  "externalProvider": "Google"
}
```

### Health Data Access
```
POST /api/googlehealth/sync
Authorization: Bearer {jwtToken}
```

**Request:**
```json
{
  "accessToken": "ya29.a0AfH6SMC7X2vjf8nP9gK...",
  "startDate": "2025-11-01T00:00:00Z",
  "endDate": "2025-11-09T23:59:59Z"
}
```

## 🛠️ Environment Variables

Create a `.env` file:

```env
REACT_APP_API_URL=http://localhost:6000
REACT_APP_GOOGLE_CLIENT_ID=699206291871-pdv6amuetllvv28eplekjabbf1rsug8m.apps.googleusercontent.com
```

## 🔧 TypeScript Types

```typescript
// src/types/auth.ts
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isExternalAuth: boolean;
  profilePicture?: string;
}

export interface AuthState {
  user: User | null;
  jwtToken: string | null;
  googleAccessToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}
```

## ✅ Testing the Integration

1. **Start your backend API**: `dotnet run` (port 6000)
2. **Start your React app**: `npm start`
3. **Click "Sign in with Google Health"**
4. **Authorize both profile and health permissions**
5. **Verify both JWT and Google access tokens are returned**
6. **Test health data endpoints**

This integration provides a seamless flow from React frontend to your .NET backend with full Google Health data access!