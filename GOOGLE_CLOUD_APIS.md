# 🔧 Google Cloud Console - Required APIs for Health Integration

## 📋 **APIs You Must Enable**

### **1. 🏥 Google Fitness API** (Primary - Most Important)
- **API Name**: `Fitness API`
- **API ID**: `fitness.googleapis.com`
- **Purpose**: Access user's health and fitness data from Google Fit
- **Required For**: All your health endpoints (heart rate, sleep, activity, etc.)

### **2. 👤 Google+ API** (For Authentication)
- **API Name**: `Google+ API`
- **API ID**: `plus.googleapis.com`
- **Purpose**: Basic user profile information (name, email, picture)
- **Required For**: User authentication and profile data

### **3. 👥 People API** (Alternative to Google+)
- **API Name**: `People API`
- **API ID**: `people.googleapis.com`
- **Purpose**: Access user's basic profile information
- **Required For**: Modern alternative to Google+ API

### **4. 🔐 Identity and Access Management (IAM) API** (Optional but Recommended)
- **API Name**: `Identity and Access Management (IAM) API`
- **API ID**: `iam.googleapis.com`
- **Purpose**: Enhanced security and access management
- **Required For**: Advanced authentication features

## 🚀 **How to Enable These APIs**

### **Method 1: Google Cloud Console (Web Interface)**

1. **Go to Google Cloud Console**: https://console.cloud.google.com/
2. **Select your project**: `symptoms-checker-health` (or your project name)
3. **Navigate to APIs & Services > Library**
4. **Search for each API and enable them**:

#### **Enable Fitness API** (MOST CRITICAL):
```
Search: "Fitness API"
Click: "Fitness API" 
Click: "ENABLE"
```

#### **Enable Google+ API**:
```
Search: "Google+ API"
Click: "Google+ API"
Click: "ENABLE"
```

#### **Enable People API**:
```
Search: "People API"  
Click: "People API"
Click: "ENABLE"
```

### **Method 2: Command Line (gcloud CLI)**

If you have Google Cloud SDK installed:

```bash
# Enable Fitness API (REQUIRED)
gcloud services enable fitness.googleapis.com

# Enable Google+ API (for profile info)
gcloud services enable plus.googleapis.com

# Enable People API (modern alternative)
gcloud services enable people.googleapis.com

# Enable IAM API (optional)
gcloud services enable iam.googleapis.com

# Verify enabled APIs
gcloud services list --enabled
```

### **Method 3: Direct Links**

Click these direct links to enable APIs:

- **Fitness API**: https://console.cloud.google.com/apis/library/fitness.googleapis.com
- **Google+ API**: https://console.cloud.google.com/apis/library/plus.googleapis.com
- **People API**: https://console.cloud.google.com/apis/library/people.googleapis.com

## ✅ **Verification Steps**

### **Check if APIs are Enabled:**

1. **Go to**: https://console.cloud.google.com/apis/dashboard
2. **You should see these APIs listed**:
   - ✅ Fitness API
   - ✅ Google+ API 
   - ✅ People API

### **Test API Access:**

After enabling, test with your application:
1. **Open**: http://localhost:3000/google-health-test.html
2. **Sign in with Google Health**
3. **Should work without API errors**

## 🚨 **Common Issues & Solutions**

### **Issue**: "API not enabled" error
**Solution**: Make sure Fitness API is enabled - this is the most critical one

### **Issue**: "Insufficient permissions" 
**Solution**: Enable Google+ API or People API for profile access

### **Issue**: Quotas exceeded
**Solution**: Check API quotas in Google Cloud Console > APIs & Services > Quotas

## 📊 **API Usage Quotas (Free Tier)**

### **Fitness API**:
- **Quota**: 1,000 requests per day (free)
- **Rate Limit**: 100 requests per 100 seconds per user
- **Cost**: Free up to quota limit

### **Google+ API**:
- **Quota**: 1,000 requests per day (free)
- **Note**: Being deprecated, use People API instead

### **People API**:
- **Quota**: 1,000 requests per day (free)  
- **Rate Limit**: 100 requests per 100 seconds per user

## 🎯 **Priority Order**

Enable in this order for your health integration:

1. **🥇 FIRST**: Fitness API (fitness.googleapis.com) - CRITICAL
2. **🥈 SECOND**: People API (people.googleapis.com) - For user profiles  
3. **🥉 THIRD**: Google+ API (plus.googleapis.com) - Backup for profiles

## 🔍 **Verification Commands**

Check if APIs are working:

```bash
# Test Fitness API access (requires access token)
curl -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
"https://www.googleapis.com/fitness/v1/users/me/dataSources"

# Check enabled services
gcloud services list --enabled --filter="name:fitness OR name:plus OR name:people"
```

---

## 📞 **Need Help?**

If you encounter issues:
1. **Check API quotas**: Console > APIs & Services > Quotas
2. **Verify billing**: Some APIs require billing enabled
3. **Check permissions**: Make sure you're project owner/editor
4. **Test with OAuth playground**: https://developers.google.com/oauthplayground/

Your Google Health integration will work once these APIs are enabled! 🚀