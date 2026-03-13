import React, { createContext, useContext, PropsWithChildren } from 'react';
import { motion } from 'framer-motion';

// ============ Context ============
interface TabsContextType {
  value: string;
  onValueChange: (value: string) => void;
}

const TabsContext = createContext<TabsContextType | undefined>(undefined);

const useTabs = () => {
  const context = useContext(TabsContext);
  if (!context) {
    throw new Error('Tabs compound components must be used within a Tabs provider');
  }
  return context;
};

// ============ Components ============

interface TabsProps extends PropsWithChildren {
  value: string;
  onValueChange: (value: string) => void;
  className?: string;
}

const Tabs: React.FC<TabsProps> = ({ value, onValueChange, children, className = '' }) => {
  return (
    <TabsContext.Provider value={{ value, onValueChange }}>
      <div className={`w-full ${className}`}>
        {children}
      </div>
    </TabsContext.Provider>
  );
};

interface TabsListProps extends PropsWithChildren {
  className?: string;
}

const TabsList: React.FC<TabsListProps> = ({ children, className = '' }) => {
  return (
    <div className={`flex border-b mb-6 overflow-x-auto ${className}`}>
        {children}
    </div>
  );
};

interface TabsTriggerProps extends PropsWithChildren {
  value: string;
  className?: string;
}

const TabsTrigger: React.FC<TabsTriggerProps> = ({ value, children, className = '' }) => {
  const { value: selectedValue, onValueChange } = useTabs();
  const isSelected = selectedValue === value;

  return (
    <button
      onClick={() => onValueChange(value)}
      className={`relative px-6 py-3 font-medium whitespace-nowrap transition-colors outline-none
        ${isSelected ? 'text-primary' : 'text-secondary hover:text-primary'}
        ${className}
      `}
    >
      {children}
      {isSelected && (
        <motion.div
          layoutId="activeTabIndicator"
          className="absolute bottom-0 left-0 right-0 h-0.5 bg-primary"
          initial={false}
          transition={{ type: "spring", stiffness: 500, damping: 30 }}
        />
      )}
    </button>
  );
};

interface TabsContentProps extends PropsWithChildren {
  value: string;
  className?: string;
}

const TabsContent: React.FC<TabsContentProps> = ({ value, children, className = '' }) => {
  const { value: selectedValue } = useTabs();
  
  if (value !== selectedValue) return null;

  return (
    <motion.div
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        exit={{ opacity: 0, y: -10 }}
        transition={{ duration: 0.2 }}
        className={className}
    >
      {children}
    </motion.div>
  );
};

// ============ Exports ============
const TabsCompound = Object.assign(Tabs, {
  List: TabsList,
  Trigger: TabsTrigger,
  Content: TabsContent,
});

export { TabsCompound as Tabs, TabsList, TabsTrigger, TabsContent };
