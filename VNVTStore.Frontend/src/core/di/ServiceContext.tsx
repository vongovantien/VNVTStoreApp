import React, { createContext, useContext, ReactNode } from 'react';

// Define the shape of your services container
export interface AppServices {
  [key: string]: unknown;
}

const ServiceContext = createContext<AppServices | null>(null);

interface ServiceProviderProps {
  services: AppServices;
  children: ReactNode;
}

export const ServiceProvider: React.FC<ServiceProviderProps> = ({ services, children }) => {
  return (
    <ServiceContext.Provider value={services}>
      {children}
    </ServiceContext.Provider>
  );
};

export const useServiceContext = () => {
  const context = useContext(ServiceContext);
  if (!context) {
    throw new Error('useServiceContext must be used within a ServiceProvider');
  }
  return context;
};
