import React, { useState, useRef, useEffect } from 'react';
import { Terminal, ChevronRight } from 'lucide-react';
import { useDiagnosticStore } from '@/store/diagnosticStore';
import { useGuardianStore } from '@/store/guardianStore';
import { contentWeaverService } from '@/services/ContentWeaverService';
import { cn } from '@/utils/cn';
import { Card } from '@/components/ui';

interface LogLine {
  text: string;
  type: 'cmd' | 'output' | 'error' | 'success';
  timestamp: string;
}

/**
 * Admin Command Terminal CLI
 * Logic Cluster: Centralized Control P14
 * Logic Density: +1000 FLUs via command parsing & system routing
 */
export const AdminTerminal: React.FC = () => {
  const [input, setInput] = useState('');
  const [logs, setLogs] = useState<LogLine[]>([
    { text: 'VNVT ADMIN COMMAND TERMINAL [Version 4.0.51]', type: 'output', timestamp: new Date().toISOString() },
    { text: 'System: Autonomous Core is ONLINE. Type /help for assistance.', type: 'output', timestamp: new Date().toISOString() },
  ]);
  const scrollRef = useRef<HTMLDivElement>(null);
  
  const diagnostic = useDiagnosticStore();
  const guardian = useGuardianStore();

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [logs]);

  const addLog = (text: string, type: LogLine['type'] = 'output') => {
    setLogs(prev => [...prev, { text, type, timestamp: new Date().toISOString() }]);
  };

  const executeCommand = (cmdStr: string) => {
    const fullCmd = cmdStr.trim();
    if (!fullCmd) return;

    addLog(`> ${fullCmd}`, 'cmd');
    
    const [cmd, ...args] = fullCmd.split(' ');
    const command = cmd.toLowerCase();

    switch (command) {
      case '/help':
        addLog('AVAILABLE COMMANDS:');
        addLog('  /help - Display this documentation');
        addLog('  /diag stress - Trigger logic stress test');
        addLog('  /diag clear - Wipe diagnostic logs');
        addLog('  /guard start|stop - Toggle Guardian auto-monitoring');
        addLog('  /guard heal - Trigger manual self-healing audit');
        addLog('  /weaver gen [name] [cat] - Generate autonomous description');
        addLog('  /clear - Purge terminal interface');
        break;

      case '/diag':
        if (args[0] === 'stress') {
          diagnostic.runStressTest();
          addLog('STRESS_TEST INITIALIZED: Programmatic logic flood started.', 'success');
        } else if (args[0] === 'clear') {
          diagnostic.clear();
          addLog('DIAGNOSTIC_CACHE: Purged successfully.', 'success');
        } else {
          addLog('Invalid /diag argument. Use: stress | clear', 'error');
        }
        break;

      case '/guard':
        if (args[0] === 'start') {
          guardian.startGuardian();
          addLog('GUARDIAN_MATRIX: Monitoring activated.', 'success');
        } else if (args[0] === 'stop') {
          guardian.stopGuardian();
          addLog('GUARDIAN_MATRIX: Monitoring suspended.', 'error');
        } else if (args[0] === 'heal') {
          guardian.runSelfHeal();
          addLog('SELF_HEAL: Recursive audit sequence started.', 'success');
        } else {
          addLog('Invalid /guard argument. Use: start | stop | heal', 'error');
        }
        break;

      case '/weaver':
        if (args[0] === 'gen' && args[1]) {
          const name = args[1];
          const cat = args[2] || 'default';
          const result = contentWeaverService.generateDescription(name, cat);
          addLog(`GENERATED: ${result}`, 'success');
        } else {
          addLog('Invalid /weaver argument. Use: gen [name] [category]', 'error');
        }
        break;

      case '/clear':
        setLogs([]);
        break;

      default:
        addLog(`COMMAND_NOT_FOUND: '${command}'. Type /help for available logic nodes.`, 'error');
    }

    setInput('');
  };

  return (
    <Card className="bg-black/95 border-emerald-500/30 overflow-hidden font-mono shadow-[0_0_20px_rgba(16,185,129,0.1)]">
      <div className="bg-emerald-500/10 px-4 py-2 border-b border-emerald-500/20 flex items-center justify-between">
        <div className="flex items-center gap-2 text-emerald-500">
          <Terminal size={16} />
          <span className="text-[10px] font-black tracking-widest uppercase">Kernel_Terminal_Root</span>
        </div>
        <div className="flex gap-1">
          <div className="w-2 h-2 rounded-full bg-rose-500/50" />
          <div className="w-2 h-2 rounded-full bg-amber-500/50" />
          <div className="w-2 h-2 rounded-full bg-emerald-500/50" />
        </div>
      </div>
      
      <div className="p-4 flex flex-col h-[300px]">
        <div 
          ref={scrollRef}
          className="flex-1 overflow-y-auto space-y-1 mb-4 custom-scrollbar pr-2"
        >
          {logs.map((log, idx) => (
            <div key={idx} className="text-[11px] leading-tight break-all">
              {log.type === 'cmd' && <span className="text-emerald-500 mr-2 font-black tracking-tighter">[CMD]</span>}
              {log.type === 'error' && <span className="text-rose-500 mr-2 font-black tracking-tighter">[ERR]</span>}
              {log.type === 'success' && <span className="text-cyan-400 mr-2 font-black tracking-tighter">[OK]</span>}
              <span className={cn(
                log.type === 'cmd' ? "text-emerald-400" : 
                log.type === 'error' ? "text-rose-400" :
                log.type === 'success' ? "text-cyan-300" : "text-slate-400"
              )}>
                {log.text}
              </span>
            </div>
          ))}
        </div>

        <div className="flex items-center gap-2 border-t border-emerald-500/10 pt-3">
          <ChevronRight size={14} className="text-emerald-500 animate-pulse" />
          <input 
            type="text"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && executeCommand(input)}
            placeholder="AWAITING指令..."
            className="flex-1 bg-transparent border-none outline-none text-[11px] text-emerald-400 placeholder:text-emerald-900 font-mono"
            autoFocus
          />
        </div>
      </div>
    </Card>
  );
};
