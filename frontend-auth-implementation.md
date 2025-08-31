# Frontend Authentication Implementation Guide

## Overview
Disaster Reporting Platform UI အတွက် secure authentication flow implementation guide

## Token Storage Strategy

### Access Token
- **Storage**: Application memory/state (React state, Vuex, etc.)
- **Lifetime**: Short-lived (15-30 minutes)
- **Usage**: API request headers
- **Security**: ❌ localStorage/sessionStorage မသုံးရ

### Refresh Token
- **Storage**: HTTP-only secure cookies (server-controlled)
- **Lifetime**: Long-lived (7-30 days)
- **Usage**: Automatic token refresh
- **Security**: Client-side မှ access မရ

## Implementation Flow

### 1. Login Process
```javascript
// Login API call
const loginResponse = await fetch('/api/auth/login', {
  method: 'POST',
  credentials: 'include', // Important for cookies
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email, password })
});

const { accessToken, expiresAt, user } = await loginResponse.json();

// Store in memory only
setAuthState({
  accessToken,
  expiresAt,
  user,
  isAuthenticated: true
});
```

### 2. API Request with Auto-Refresh
```javascript
const apiCall = async (url, options = {}) => {
  // Add access token to headers
  const headers = {
    ...options.headers,
    'Authorization': `Bearer ${authState.accessToken}`
  };

  let response = await fetch(url, {
    ...options,
    headers,
    credentials: 'include'
  });

  // Handle token expiration
  if (response.status === 401) {
    const refreshed = await refreshToken();
    if (refreshed) {
      // Retry with new token
      headers['Authorization'] = `Bearer ${authState.accessToken}`;
      response = await fetch(url, { ...options, headers, credentials: 'include' });
    } else {
      // Redirect to login
      redirectToLogin();
      return;
    }
  }

  return response;
};
```

### 3. Token Refresh
```javascript
const refreshToken = async () => {
  try {
    const response = await fetch('/api/auth/refresh', {
      method: 'POST',
      credentials: 'include' // Sends refresh token cookie
    });

    if (response.ok) {
      const { accessToken, expiresAt } = await response.json();
      
      // Update memory state
      setAuthState(prev => ({
        ...prev,
        accessToken,
        expiresAt
      }));
      
      return true;
    }
    
    return false;
  } catch (error) {
    console.error('Token refresh failed:', error);
    return false;
  }
};
```

### 4. Background Auto-Refresh
```javascript
const setupAutoRefresh = () => {
  const checkAndRefresh = async () => {
    if (!authState.isAuthenticated) return;
    
    const now = Date.now();
    const expiresAt = new Date(authState.expiresAt).getTime();
    const timeUntilExpiry = expiresAt - now;
    
    // Refresh 5 minutes before expiry
    if (timeUntilExpiry < 5 * 60 * 1000) {
      await refreshToken();
    }
  };

  // Check every minute
  const interval = setInterval(checkAndRefresh, 60 * 1000);
  
  return () => clearInterval(interval);
};
```

### 5. Logout Process
```javascript
const logout = async () => {
  try {
    // Call server to revoke refresh token
    await fetch('/api/auth/logout', {
      method: 'POST',
      credentials: 'include'
    });
  } catch (error) {
    console.error('Logout API failed:', error);
  } finally {
    // Always clear local state
    setAuthState({
      accessToken: null,
      expiresAt: null,
      user: null,
      isAuthenticated: false
    });
    
    // Redirect to login
    redirectToLogin();
  }
};
```

## React Implementation Example

### Auth Context
```javascript
const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [authState, setAuthState] = useState({
    accessToken: null,
    expiresAt: null,
    user: null,
    isAuthenticated: false
  });

  useEffect(() => {
    const cleanup = setupAutoRefresh();
    return cleanup;
  }, [authState.isAuthenticated]);

  const value = {
    ...authState,
    login,
    logout,
    refreshToken,
    apiCall
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};
```

### Protected Route
```javascript
const ProtectedRoute = ({ children }) => {
  const { isAuthenticated } = useAuth();
  
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  
  return children;
};
```

## Security Best Practices

1. **Never store tokens in localStorage/sessionStorage**
2. **Always use `credentials: 'include'` for cookie-based requests**
3. **Implement proper error handling for token refresh failures**
4. **Clear all auth state on logout**
5. **Use HTTPS in production**
6. **Implement CSRF protection if needed**

## API Endpoints Required

- `POST /api/auth/login` - Returns access token, sets refresh token cookie
- `POST /api/auth/refresh` - Returns new access token using cookie
- `POST /api/auth/logout` - Clears refresh token cookie
- `POST /api/auth/signup` - Same as login
- `POST /api/auth/google-login` - Same as login

## Error Handling

```javascript
const handleAuthError = (error, response) => {
  if (response?.status === 401) {
    // Token expired or invalid
    logout();
  } else if (response?.status === 403) {
    // Insufficient permissions
    showErrorMessage('Access denied');
  } else {
    // Network or other errors
    showErrorMessage('Authentication failed');
  }
};
```

This implementation ensures secure token management while providing a smooth user experience with automatic token refresh and proper error handling.