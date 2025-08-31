# Working SignalR Implementation Fix

## Issue Analysis
- SignalR connection established ✅
- Backend sending `ChartDataUpdated` events ✅ 
- Frontend not receiving/processing data updates ❌
- Stats cards showing 0 values ❌

## Quick Fix Implementation

### 1. Create SignalR Service (`signalRService.js`)

```javascript
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

class SignalRService {
  constructor() {
    this.connection = null;
    this.isConnected = false;
  }

  async startConnection(getToken) {
    try {
      if (this.connection?.state === 'Connected') {
        return;
      }

      const token = getToken();
      if (!token) {
        throw new Error('No access token available');
      }

      this.connection = new HubConnectionBuilder()
        .withUrl('http://localhost:5057/userStatsHub', {
          accessTokenFactory: () => token,
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      // Connection events
      this.connection.onclose(() => {
        this.isConnected = false;
        console.log('SignalR: Connection closed');
      });

      this.connection.onreconnected(() => {
        this.isConnected = true;
        console.log('SignalR: Reconnected');
        this.joinUserManagementGroup();
      });

      await this.connection.start();
      this.isConnected = true;
      console.log('SignalR: Connected successfully');

      await this.joinUserManagementGroup();
      
      // Request initial data
      setTimeout(() => {
        this.requestDataRefresh();
      }, 1000);

    } catch (error) {
      console.error('SignalR: Failed to start connection', error);
      this.isConnected = false;
    }
  }

  async joinUserManagementGroup() {
    if (this.connection?.state === 'Connected') {
      try {
        await this.connection.invoke('JoinUserManagementGroup');
        console.log('SignalR: Joined user management group');
      } catch (error) {
        console.error('SignalR: Failed to join group', error);
      }
    }
  }

  async requestDataRefresh() {
    if (this.connection?.state === 'Connected') {
      try {
        await this.connection.invoke('RequestDataRefresh');
        console.log('SignalR: Requested data refresh');
      } catch (error) {
        console.error('SignalR: Failed to request data refresh', error);
      }
    }
  }

  // Event listeners
  onChartDataUpdated(callback) {
    if (this.connection) {
      this.connection.on('chartdataupdated', (data) => {
        console.log('SignalR: Chart data received:', data);
        callback(data);
      });
    }
  }

  onUserStatsUpdated(callback) {
    if (this.connection) {
      this.connection.on('userstatsupdated', (data) => {
        console.log('SignalR: User stats received:', data);
        callback(data);
      });
    }
  }

  async stopConnection() {
    if (this.connection) {
      await this.connection.stop();
      this.isConnected = false;
    }
  }
}

export const signalRService = new SignalRService();
```

### 2. Create React Hook (`useSignalRData.js`)

```javascript
import { useState, useEffect, useCallback } from 'react';
import { signalRService } from '../services/signalRService';

export const useSignalRData = (getToken) => {
  const [chartData, setChartData] = useState(null);
  const [userStats, setUserStats] = useState({
    totalUsers: 0,
    activeUsers: 0,
    suspendedUsers: 0,
    newJoins: 0
  });
  const [isConnected, setIsConnected] = useState(false);
  const [lastUpdated, setLastUpdated] = useState(null);

  const handleChartDataUpdate = useCallback((data) => {
    console.log('Processing chart data update:', data);
    
    // Transform backend data to frontend format
    const transformedData = {
      monthlyData: data.monthlyData?.map(item => ({
        month: item.month,
        users: item.activeUsers + item.suspendedUsers,
        newUsers: item.newJoins,
        activeUsers: item.activeUsers,
        suspendedUsers: item.suspendedUsers
      })) || [],
      roleDistribution: [
        { role: 'Admin', count: data.roleDistribution?.admin || 0 },
        { role: 'CJ', count: data.roleDistribution?.cj || 0 },
        { role: 'User', count: data.roleDistribution?.user || 0 }
      ]
    };

    setChartData(transformedData);
    setLastUpdated(new Date());
  }, []);

  const handleUserStatsUpdate = useCallback((data) => {
    console.log('Processing user stats update:', data);
    
    setUserStats({
      totalUsers: data.totalUsers || 0,
      activeUsers: data.activeUsers || 0,
      suspendedUsers: data.suspendedUsers || 0,
      newJoins: data.newJoins || 0
    });
    setLastUpdated(new Date());
  }, []);

  const refreshData = useCallback(async () => {
    if (signalRService.isConnected) {
      await signalRService.requestDataRefresh();
    }
  }, []);

  useEffect(() => {
    const initializeSignalR = async () => {
      try {
        await signalRService.startConnection(getToken);
        setIsConnected(true);

        // Set up event listeners
        signalRService.onChartDataUpdated(handleChartDataUpdate);
        signalRService.onUserStatsUpdated(handleUserStatsUpdate);

      } catch (error) {
        console.error('Failed to initialize SignalR:', error);
        setIsConnected(false);
      }
    };

    initializeSignalR();

    return () => {
      signalRService.stopConnection();
      setIsConnected(false);
    };
  }, [getToken, handleChartDataUpdate, handleUserStatsUpdate]);

  return {
    chartData,
    userStats,
    isConnected,
    lastUpdated,
    refreshData
  };
};
```

### 3. Update Your Chart Component

```javascript
import React from 'react';
import { useSignalRData } from '../hooks/useSignalRData';

export const UserManagementCharts = () => {
  const getToken = () => localStorage.getItem('access_token'); // Adjust to your token storage
  
  const {
    chartData,
    userStats,
    isConnected,
    lastUpdated,
    refreshData
  } = useSignalRData(getToken);

  return (
    <div className="p-6">
      {/* Connection Status */}
      <div className="mb-4 flex justify-between items-center">
        <h2 className="text-2xl font-bold">User Management Analytics</h2>
        <div className="flex items-center space-x-4">
          <div className={`flex items-center space-x-2 ${isConnected ? 'text-green-600' : 'text-red-600'}`}>
            <span>{isConnected ? '🟢' : '🔴'}</span>
            <span className="text-sm">{isConnected ? 'Connected' : 'Disconnected'}</span>
          </div>
          <button 
            onClick={refreshData}
            className="px-3 py-1 bg-blue-500 text-white rounded text-sm hover:bg-blue-600"
          >
            Refresh
          </button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <div className="bg-blue-100 p-4 rounded-lg">
          <h3 className="text-sm font-medium text-blue-800">Total Users</h3>
          <p className="text-2xl font-bold text-blue-900">{userStats.totalUsers}</p>
        </div>
        <div className="bg-green-100 p-4 rounded-lg">
          <h3 className="text-sm font-medium text-green-800">Active Users</h3>
          <p className="text-2xl font-bold text-green-900">{userStats.activeUsers}</p>
        </div>
        <div className="bg-red-100 p-4 rounded-lg">
          <h3 className="text-sm font-medium text-red-800">Suspended</h3>
          <p className="text-2xl font-bold text-red-900">{userStats.suspendedUsers}</p>
        </div>
        <div className="bg-yellow-100 p-4 rounded-lg">
          <h3 className="text-sm font-medium text-yellow-800">New This Month</h3>
          <p className="text-2xl font-bold text-yellow-900">{userStats.newJoins}</p>
        </div>
      </div>

      {/* Last Updated */}
      {lastUpdated && (
        <div className="mb-4 text-sm text-gray-500">
          Last updated: {lastUpdated.toLocaleString()}
          {isConnected && <span className="ml-2 text-green-600">(Live)</span>}
        </div>
      )}

      {/* Charts */}
      {chartData && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Monthly Data */}
          <div className="bg-white p-4 rounded-lg shadow">
            <h3 className="text-lg font-semibold mb-3">Monthly User Activity</h3>
            <div className="space-y-2">
              {chartData.monthlyData.map((item, index) => (
                <div key={index} className="flex justify-between items-center py-2 border-b">
                  <span className="font-medium">{item.month}</span>
                  <div className="flex space-x-4 text-sm">
                    <span>Total: {item.users}</span>
                    <span className="text-green-600">Active: {item.activeUsers}</span>
                    <span className="text-red-600">Suspended: {item.suspendedUsers}</span>
                    <span className="text-blue-600">New: {item.newUsers}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Role Distribution */}
          <div className="bg-white p-4 rounded-lg shadow">
            <h3 className="text-lg font-semibold mb-3">Role Distribution</h3>
            <div className="space-y-2">
              {chartData.roleDistribution.map((item, index) => (
                <div key={index} className="flex justify-between items-center py-2 border-b">
                  <span className="font-medium">{item.role}</span>
                  <span className="text-lg font-bold">{item.count}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {!chartData && (
        <div className="text-center py-8 text-gray-500">
          {isConnected ? 'Loading chart data...' : 'Connect to view real-time data'}
        </div>
      )}
    </div>
  );
};
```

## Backend Verification

Make sure your backend is sending both events. In your `UserStatsHubService.cs`, ensure you're calling:

```csharp
// Send both user stats and chart data
await _hubContext.Clients.Group("UserManagement").SendAsync("UserStatsUpdated", userStatsUpdate);
await _hubContext.Clients.Group("UserManagement").SendAsync("ChartDataUpdated", chartDataUpdate);
```

## Testing the Fix

1. **Install dependencies**: `npm install @microsoft/signalr`
2. **Replace your current SignalR implementation** with the code above
3. **Open browser console** to see SignalR logs
4. **Check that data is being received** in console logs
5. **Verify stats cards update** with real values
6. **Test real-time updates** by making changes in admin panel

## Expected Console Output

When working correctly, you should see:
```
SignalR: Connected successfully
SignalR: Joined user management group
SignalR: Requested data refresh
SignalR: Chart data received: {monthlyData: [...], roleDistribution: {...}}
SignalR: User stats received: {totalUsers: 4, activeUsers: 3, suspendedUsers: 1, newJoins: 0}
Processing chart data update: {...}
Processing user stats update: {...}
```

This implementation will fix both the real-time chart updates and populate the stats cards with actual values.
