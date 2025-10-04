# Google Authentication Setup Guide

This guide explains how to set up Google OAuth authentication for the Symptoms Checker API.

## Prerequisites

1. Google Cloud Console account
2. A Google Cloud Project

## Step 1: Create Google OAuth Credentials

1. **Go to Google Cloud Console**
   - Visit [Google Cloud Console](https://console.cloud.google.com/)
   - Select your project or create a new one

2. **Enable Google+ API**
   - Go to "APIs & Services" > "Library"
   - Search for "Google+ API" and enable it
   - Also enable "Google Identity and Access Management (IAM) API"

3. **Create OAuth 2.0 Credentials**
   - Go to "APIs & Services" > "Credentials"
   - Click "Create Credentials" > "OAuth 2.0 Client ID"
   - Select "Web application" as the application type
   - Add your authorized origins (e.g., `http://localhost:3000` for development)
   - Add authorized redirect URIs if needed
   - Click "Create"

4. **Get Your Credentials**
   - Copy the Client ID and Client Secret
   - Update your `appsettings.json` and `appsettings.Development.json` files

## Step 2: Configure API Settings

Update your `appsettings.json`:

```json
{
  "Google": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-google-client-secret"
  }
}
```

Update your `appsettings.Development.json`:

```json
{
  "Google": {
    "ClientId": "your-development-google-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-development-google-client-secret"
  }
}
```

## Step 3: Frontend Integration

### Using Google Identity Services Library

1. **Include Google Identity Services in your HTML:**

```html
<script src="https://accounts.google.com/gsi/client" async defer></script>
```

2. **Initialize Google Sign-In:**

```javascript
function initializeGoogleSignIn() {
  google.accounts.id.initialize({
    client_id: 'your-google-client-id.apps.googleusercontent.com',
    callback: handleGoogleSignIn
  });
  
  google.accounts.id.renderButton(
    document.getElementById('google-signin-button'),
    { 
      theme: 'outline', 
      size: 'large',
      text: 'signin_with'
    }
  );
}

function handleGoogleSignIn(response) {
  // Send the ID token to your backend
  fetch('/api/auth/google', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      idToken: response.credential
    })
  })
  .then(response => response.json())
  .then(data => {
    if (data.token) {
      // Store the JWT token
      localStorage.setItem('authToken', data.token);
      // Redirect to dashboard or update UI
      console.log('Authentication successful:', data);
    }
  })
  .catch(error => {
    console.error('Authentication failed:', error);
  });
}
```

3. **React Integration Example:**

```jsx
import { GoogleLogin } from '@react-oauth/google';

function LoginComponent() {
  const handleGoogleSuccess = async (credentialResponse) => {
    try {
      const response = await fetch('/api/auth/google', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          idToken: credentialResponse.credential,
        }),
      });

      const data = await response.json();
      
      if (response.ok) {
        // Store token and redirect
        localStorage.setItem('authToken', data.token);
        console.log('Google auth successful:', data);
      } else {
        console.error('Google auth failed:', data.message);
      }
    } catch (error) {
      console.error('Google auth error:', error);
    }
  };

  const handleGoogleError = () => {
    console.error('Google Sign-In failed');
  };

  return (
    <GoogleLogin
      onSuccess={handleGoogleSuccess}
      onError={handleGoogleError}
    />
  );
}
```

## API Endpoints

### Google Authentication

**POST `/api/auth/google`**

Request body:
```json
{
  "idToken": "google-id-token-from-frontend",
  "firstName": "John",     // Optional
  "lastName": "Doe",       // Optional
  "email": "john@example.com",  // Optional
  "picture": "https://..."      // Optional
}
```

Response:
```json
{
  "token": "jwt-access-token",
  "refreshToken": "refresh-token",
  "expiration": "2024-01-01T00:00:00Z",
  "isNewUser": true,
  "user": {
    "id": "user-id",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "0001-01-01T00:00:00",
    "gender": null,
    "profilePicture": "https://...",
    "isExternalAuth": true
  }
}
```

### Generic External Authentication

**POST `/api/auth/external`**

Request body:
```json
{
  "provider": "google",
  "idToken": "provider-id-token",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "picture": "https://..."
}
```

## Security Considerations

1. **Token Validation**: The API validates Google ID tokens server-side using Google's libraries
2. **Email Verification**: Only verified email addresses are accepted
3. **User Linking**: Existing users can be linked to Google accounts
4. **Profile Updates**: Google profile information updates existing user data

## Testing

1. **Development Testing**:
   - Use Google's test accounts
   - Configure localhost origins in Google Console
   - Test with both new and existing users

2. **Production Testing**:
   - Update origins to production URLs
   - Use production Google OAuth credentials
   - Test cross-platform compatibility

## Troubleshooting

### Common Issues:

1. **"Invalid token" errors**:
   - Verify Client ID matches in both frontend and backend
   - Check token expiration
   - Ensure proper audience validation

2. **CORS errors**:
   - Add your domain to Google Console origins
   - Configure CORS in your API

3. **Email not verified**:
   - Google tokens must have verified emails
   - Check user's Google account verification status

### Debug Mode:

Enable detailed logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "SymptomsCheckerApi.Services.GoogleAuthService": "Debug",
      "Google": "Debug"
    }
  }
}
```

## Additional Features

The Google authentication system supports:

- **Automatic user creation** for new Google users
- **Account linking** for existing email/password users
- **Profile picture sync** from Google accounts
- **Graceful fallback** to email/password authentication
- **JWT token generation** with user claims
- **Refresh token support** for long-term sessions