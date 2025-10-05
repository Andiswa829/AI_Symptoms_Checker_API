# 🏥 Google Health Integration Documentation

## Overview

The Symptoms Checker API now includes comprehensive **Google Health/Fit API integration** that allows users to sync and retrieve their health data from Google Health services. This integration provides access to various health metrics including heart rate, blood pressure, sleep data, activity tracking, and more.

## 🔧 Features

### ✅ Implemented Health Data Types

| Health Metric | Endpoint | Description |
|--------------|----------|-------------|
| ❤️ **Heart Rate** | `/api/googlehealth/heart-rate` | Current, resting, max, min heart rate data |
| 🩺 **Blood Pressure** | `/api/googlehealth/blood-pressure` | Systolic/diastolic readings with medical categorization |
| 🫁 **Oxygen Saturation** | `/api/googlehealth/oxygen-saturation` | SpO2 measurements and normal range indicators |
| 🌡️ **Body Temperature** | `/api/googlehealth/body-temperature` | Body temperature readings and fever detection |
| 😴 **Sleep Data** | `/api/googlehealth/sleep` | Sleep duration, quality, stages, and efficiency |
| 🏃‍♂️ **Activity Data** | `/api/googlehealth/activity` | Steps, distance, calories, exercise sessions |
| 😰 **Stress Levels** | `/api/googlehealth/stress-level` | Estimated stress levels from HRV analysis |
| 📊 **Comprehensive Sync** | `/api/googlehealth/sync` | All health data in one API call |

## 🔑 Authentication & Setup

### Required Google OAuth Scopes

To access Google Health data, your application must request the following OAuth scopes:

```javascript
const REQUIRED_SCOPES = [
    'https://www.googleapis.com/auth/fitness.read',
    'https://www.googleapis.com/auth/fitness.body.read',
    'https://www.googleapis.com/auth/fitness.activity.read',
    'https://www.googleapis.com/auth/fitness.sleep.read'
];
```

### Frontend Integration Example

```javascript
// 1. Initialize Google Sign-In with health scopes
function initializeGoogleAuth() {
    gapi.load('auth2', function() {
        gapi.auth2.init({
            client_id: '699206291871-pdv6amuetllvv28eplekjabbf1rsug8m.apps.googleusercontent.com',
            scope: REQUIRED_SCOPES.join(' ')
        });
    });
}

// 2. Sign in and get access token
async function signInWithGoogle() {
    const authInstance = gapi.auth2.getAuthInstance();
    const user = await authInstance.signIn({
        scope: REQUIRED_SCOPES.join(' ')
    });
    
    const authResponse = user.getAuthResponse();
    return {
        idToken: authResponse.id_token,
        accessToken: authResponse.access_token
    };
}

// 3. Authenticate with your API
async function authenticateWithApi(idToken) {
    const response = await fetch('/api/auth/google', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            idToken: idToken
        })
    });
    
    const data = await response.json();
    return data.token; // JWT token for API access
}

// 4. Sync health data
async function syncHealthData(jwtToken, googleAccessToken) {
    const response = await fetch('/api/googlehealth/sync', {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${jwtToken}`,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            accessToken: googleAccessToken,
            dataTypes: ['heart_rate', 'blood_pressure', 'sleep', 'activity']
        })
    });
    
    return await response.json();
}
```

## 📊 API Endpoints

### 🔄 Comprehensive Health Data Sync

**POST** `/api/googlehealth/sync`

Retrieves all available health metrics in a single API call.

**Request Body:**
```json
{
    "accessToken": "ya29.a0AfH6SMC...",
    "startDate": "2025-09-28T00:00:00Z",
    "endDate": "2025-10-05T23:59:59Z",
    "dataTypes": [
        "heart_rate",
        "blood_pressure", 
        "oxygen_saturation",
        "body_temperature",
        "sleep",
        "activity",
        "stress_level"
    ]
}
```

**Response Example:**
```json
{
    "heartRate": {
        "currentHeartRate": 72,
        "restingHeartRate": 65,
        "maxHeartRate": 180,
        "minHeartRate": 58,
        "averageHeartRate": 68,
        "readings": [
            {
                "value": 72,
                "timestamp": "2025-10-05T14:30:00Z",
                "source": "Wear OS by Google"
            }
        ],
        "dataSource": "com.google.heart_rate.bpm"
    },
    "bloodPressure": {
        "latestSystolic": 120,
        "latestDiastolic": 80,
        "category": "Normal",
        "readings": [
            {
                "systolic": 120,
                "diastolic": 80,
                "timestamp": "2025-10-05T08:00:00Z",
                "source": "Blood Pressure Monitor"
            }
        ]
    },
    "sleep": {
        "totalSleepTime": 450,
        "sleepEfficiency": 0.85,
        "bedTime": "2025-10-04T23:00:00Z",
        "wakeTime": "2025-10-05T07:30:00Z",
        "sleepStages": {
            "deep": 90,
            "light": 240,
            "rem": 120,
            "awake": 30
        }
    }
}
```

### ❤️ Individual Health Metric Endpoints

Each health metric has its own endpoint for targeted data retrieval:

| Endpoint | Method | Parameters |
|----------|---------|------------|
| `/api/googlehealth/heart-rate` | GET | `accessToken`, `startDate?`, `endDate?` |
| `/api/googlehealth/blood-pressure` | GET | `accessToken`, `startDate?`, `endDate?` |
| `/api/googlehealth/oxygen-saturation` | GET | `accessToken`, `startDate?`, `endDate?` |
| `/api/googlehealth/body-temperature` | GET | `accessToken`, `startDate?`, `endDate?` |
| `/api/googlehealth/sleep` | GET | `accessToken`, `startDate?`, `endDate?` |
| `/api/googlehealth/activity` | GET | `accessToken`, `startDate?`, `endDate?` |
| `/api/googlehealth/stress-level` | GET | `accessToken`, `startDate?`, `endDate?` |

### 🔐 Token Validation

**POST** `/api/googlehealth/validate-token`

Validates Google OAuth access token and checks for required scopes.

**Request Body:**
```json
"ya29.a0AfH6SMC..."
```

**Response:**
```json
{
    "valid": true,
    "message": "Access token is valid and has required health scopes"
}
```

## 📈 Health Data Models

### Heart Rate Data
```json
{
    "currentHeartRate": 72,
    "restingHeartRate": 65,
    "maxHeartRate": 180,
    "minHeartRate": 58,
    "averageHeartRate": 68,
    "readings": [
        {
            "value": 72,
            "timestamp": "2025-10-05T14:30:00Z",
            "source": "Wear OS by Google"
        }
    ],
    "dataSource": "com.google.heart_rate.bpm",
    "lastUpdated": "2025-10-05T14:30:00Z"
}
```

### Blood Pressure Data
```json
{
    "latestSystolic": 120,
    "latestDiastolic": 80,
    "category": "Normal",
    "readings": [
        {
            "systolic": 120,
            "diastolic": 80,
            "timestamp": "2025-10-05T08:00:00Z",
            "source": "Blood Pressure Monitor"
        }
    ],
    "averageSystolic": 118,
    "averageDiastolic": 78,
    "dataSource": "com.google.blood_pressure",
    "lastUpdated": "2025-10-05T08:00:00Z"
}
```

### Sleep Data
```json
{
    "totalSleepTime": 450,
    "sleepEfficiency": 0.85,
    "bedTime": "2025-10-04T23:00:00Z",
    "wakeTime": "2025-10-05T07:30:00Z",
    "sleepStages": {
        "deep": 90,
        "light": 240,
        "rem": 120,
        "awake": 30
    },
    "sleepQualityScore": 85,
    "sessions": [
        {
            "startTime": "2025-10-04T23:00:00Z",
            "endTime": "2025-10-05T07:30:00Z",
            "duration": 450,
            "efficiency": 0.85
        }
    ],
    "dataSource": "com.google.sleep.segment",
    "lastUpdated": "2025-10-05T07:30:00Z"
}
```

### Activity Data
```json
{
    "steps": 8542,
    "distance": 6.2,
    "caloriesBurned": 285,
    "activeMinutes": 45,
    "activities": [
        {
            "type": "Walking",
            "duration": 30,
            "distance": 2.5,
            "caloriesBurned": 120,
            "startTime": "2025-10-05T08:00:00Z"
        }
    ],
    "stepGoal": 10000,
    "goalProgress": 0.85,
    "dataSource": "com.google.step_count.delta",
    "lastUpdated": "2025-10-05T18:00:00Z"
}
```

## 🔄 Data Flow

1. **User Authentication**: User signs in with Google OAuth including health scopes
2. **Token Exchange**: Frontend exchanges Google ID token for JWT with `/api/auth/google`
3. **Health Data Request**: Use JWT + Google access token to call health endpoints
4. **Data Processing**: API fetches data from Google Fit, processes and returns structured response
5. **Frontend Integration**: Display health metrics in your application UI

## ⚡ Performance Considerations

- **Parallel Fetching**: The `/sync` endpoint fetches different health metrics in parallel
- **Caching**: Consider implementing caching for frequently accessed health data
- **Rate Limits**: Google Fit API has rate limits - implement appropriate retry logic
- **Data Freshness**: Health data may have different update frequencies from various devices

## 🛠️ Error Handling

The API implements comprehensive error handling:

| Error Code | Description | Response |
|------------|-------------|----------|
| **400** | Invalid request parameters | `{ "message": "Invalid or expired Google Health access token" }` |
| **401** | Unauthorized - invalid JWT | `{ "message": "User ID not found in token" }` |
| **403** | Insufficient Google Health permissions | `"Insufficient Google Health API permissions"` |
| **404** | No health data found | `{ "message": "No heart rate data found for the specified period" }` |
| **500** | Internal server error | `{ "message": "An error occurred while syncing health data" }` |

## 🧪 Testing

Use the provided `SymptomsCheckerApi-GoogleHealth.http` file for testing all endpoints:

1. **Register/Login** to get JWT token
2. **Validate Google token** before making health requests
3. **Test individual endpoints** or use comprehensive sync
4. **Verify error handling** with invalid tokens

## 🔮 Future Enhancements

- **Historical Trends**: Advanced analytics and trend analysis
- **Health Insights**: AI-powered health recommendations
- **Real-time Notifications**: Alerts for abnormal health metrics
- **Wearable Integration**: Direct integration with specific wearable devices
- **Health Goals**: Personal health goal setting and tracking
- **Export Functionality**: Export health data to various formats

## 📚 Additional Resources

- [Google Fit REST API Documentation](https://developers.google.com/fit/rest)
- [Google OAuth 2.0 Scopes for Fitness API](https://developers.google.com/fit/rest/v1/authorization)
- [ASP.NET Core Authentication Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)

---

**🎉 Your Symptoms Checker API now has comprehensive Google Health integration!** 

Users can seamlessly sync their health data from Google Health/Fit and use it for symptom analysis, health tracking, and personalized healthcare insights.