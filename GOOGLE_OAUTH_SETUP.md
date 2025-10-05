# 🔧 Google OAuth Setup for Health Data Integration

## 🎯 **Complete Google Health OAuth Setup Guide**

### **Step 1: Google Cloud Console Configuration**

#### **1.1 Create/Configure Google Cloud Project**
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Select your existing project or create a new one
3. **Project ID**: `symptoms-checker-health` (or your preferred name)

#### **1.2 Enable Required APIs**
Navigate to **APIs & Services > Library** and enable:
- ✅ **Google Fit API** (`fitness.googleapis.com`)
- ✅ **Google+ API** (`plus.googleapis.com`) - for basic profile
- ✅ **People API** (`people.googleapis.com`) - for profile information

```bash
# Enable APIs via CLI (optional)
gcloud services enable fitness.googleapis.com
gcloud services enable plus.googleapis.com
gcloud services enable people.googleapis.com
```

#### **1.3 Configure OAuth Consent Screen**
Go to **APIs & Services > OAuth consent screen**:

**Basic Information:**
- **Application name**: `Symptoms Checker Health`
- **User support email**: `your-email@example.com`
- **Application logo**: Upload your app logo (optional)
- **Application home page**: `http://localhost:3000` (development)
- **Application privacy policy**: `http://localhost:3000/privacy` (create page)
- **Application terms of service**: `http://localhost:3000/terms` (create page)

**Scopes Configuration:**
Add these **sensitive scopes**:
- `../auth/fitness.read` - Read fitness data
- `../auth/fitness.body.read` - Read body measurements  
- `../auth/fitness.activity.read` - Read activity data
- `../auth/fitness.sleep.read` - Read sleep data

#### **1.4 Create OAuth 2.0 Credentials**
Go to **APIs & Services > Credentials > Create Credentials > OAuth 2.0 Client ID**:

**Application Type**: `Web application`

**Authorized JavaScript origins:**
```
http://localhost:3000
http://localhost:3001
http://localhost:5173
http://localhost:4200
https://yourdomain.com
```

**Authorized redirect URIs:**
```
http://localhost:3000/auth/callback
http://localhost:3001/auth/callback
https://yourdomain.com/auth/callback
```

**Save the credentials:**
- **Client ID**: `699206291871-xxxxxxxx.apps.googleusercontent.com`
- **Client Secret**: `GOCSPX-xxxxxxxx` (keep secret!)

### **Step 2: Update Your ASP.NET Core Configuration**

#### **2.1 Update appsettings.json**