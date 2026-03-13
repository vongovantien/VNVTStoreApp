import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Save } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { AvatarUpload } from '@/components/common';
import { createSchemas } from '@/utils/schemas';
import { getImageUrl } from '@/utils/format';

export interface UpdateProfileData {
  fullName: string;
  email: string;
  phone: string;
  avatarUrl?: string | undefined;
}

interface ProfileFormProps {
  initialData: {
    fullName: string;
    email: string;
    phone: string;
    avatar?: string;
  };
  onSave: (data: UpdateProfileData) => Promise<void>;
  loading: boolean;
}

const ProfileForm = ({ initialData, onSave, loading }: ProfileFormProps) => {
  const { t } = useTranslation();
  const { userBaseSchema } = createSchemas(t);
  const avatarVal = (initialData.avatar || (initialData as Record<string, unknown>).Avatar) as string | undefined;
  const [avatarUrl, setAvatarUrl] = useState<string | undefined>(getImageUrl(avatarVal) || undefined);

  const form = useForm({
    resolver: zodResolver(userBaseSchema),
    defaultValues: initialData
  });

  const [prevAvatar, setPrevAvatar] = useState(initialData.avatar);

  // Handle initialData changes (e.g. after fetch)
  if (avatarVal !== prevAvatar) {
    setPrevAvatar(avatarVal);
    setAvatarUrl(getImageUrl(avatarVal));
  }

  useEffect(() => {
    form.reset(initialData);
  }, [initialData, form]);

  const handleSubmit = (data: { fullName: string; email: string; phone: string }) => {
    onSave({ ...data, avatarUrl });
  };

  return (
    <section className="bg-primary rounded-[2rem] p-6 md:p-10 border border-secondary/5 shadow-2xl shadow-indigo-500/5 animate-fade-in overflow-hidden relative">
      <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-indigo-500 via-purple-500 to-blue-500 opacity-70"></div>
      
      <div className="flex flex-col md:flex-row items-center justify-between mb-10 pb-6 border-b border-secondary/5 gap-4">
        <div className="text-center md:text-left">
          <h2 className="text-3xl font-extrabold text-primary tracking-tight lg:text-4xl">
            {t('common.account.profile')}
          </h2>
          <p className="text-sm text-tertiary mt-2 font-medium">
            {t('common.account.profileSubtitle') || 'Manage your personal identity and security settings'}
          </p>
        </div>
        <div className="flex items-center gap-3">
           <div className="px-4 py-1.5 bg-indigo-50 dark:bg-indigo-900/30 rounded-full text-[11px] font-bold text-indigo-600 dark:text-indigo-400 uppercase tracking-widest border border-indigo-100 dark:border-indigo-800 shadow-sm">
             {t('common.status.active') || 'Verified Account'}
           </div>
        </div>
      </div>
      
      <div className="flex flex-col lg:flex-row gap-12 xl:gap-20">
        {/* Left: Avatar Section */}
        <div className="flex flex-col items-center shrink-0">
           <div className="relative group p-2 rounded-full bg-slate-50 dark:bg-slate-900 shadow-inner border border-secondary/5 mb-6">
            <AvatarUpload 
                currentAvatarUrl={avatarUrl}
                onUploadSuccess={setAvatarUrl}
                size="xl"
                className="transform transition-transform duration-500 group-hover:scale-[1.02]"
            />
           </div>
           <div className="space-y-3 text-center bg-secondary/5 p-4 rounded-2xl border border-secondary/5 w-full max-w-[240px]">
              <p className="text-xs font-bold text-primary uppercase tracking-tighter">
                {t('common.account.avatar') || 'Identity Photo'}
              </p>
              <p className="text-[11px] text-tertiary leading-relaxed px-2 font-medium">
                {t('common.account.avatarHint') || 'Recommended: Square JPG, PNG or WebP. Max size 10MB.'}
              </p>
           </div>
        </div>

        {/* Right: Form Section */}
        <form onSubmit={form.handleSubmit(handleSubmit)} className="flex-1 space-y-8">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div className="space-y-2">
              <label className="text-xs font-bold text-tertiary uppercase tracking-wider ml-1">{t('common.fields.fullName') || t('common.fields.name')}</label>
              <Input
                placeholder={t('common.placeholders.fullName')}
                {...form.register('fullName')}
                error={form.formState.errors.fullName?.message as string}
                className="bg-secondary/10 border-none focus:ring-2 focus:ring-indigo-500/20 transition-all py-3 rounded-xl font-medium"
              />
            </div>

            <div className="space-y-2">
              <label className="text-xs font-bold text-tertiary uppercase tracking-wider ml-1">{t('common.fields.email')}</label>
              <div className="relative group">
                <Input
                  type="email"
                  {...form.register('email')}
                  readOnly
                  disabled
                  className="bg-secondary/5 border-secondary/10 cursor-not-allowed opacity-60 text-tertiary py-3 rounded-xl font-medium pl-10"
                  placeholder={t('common.placeholders.email')}
                />
                <div className="absolute left-3 top-1/2 -translate-y-1/2 text-tertiary opacity-50 group-hover:opacity-100 transition-opacity">
                   <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
                </div>
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-xs font-bold text-tertiary uppercase tracking-wider ml-1">{t('common.fields.phone')}</label>
              <Input
                type="tel"
                placeholder={t('common.placeholders.phone')}
                {...form.register('phone')}
                error={form.formState.errors.phone?.message as string}
                className="bg-secondary/10 border-none focus:ring-2 focus:ring-indigo-500/20 transition-all py-3 rounded-xl font-medium"
              />
            </div>
          </div>
          
          <div className="pt-8 border-t border-secondary/5 flex justify-end">
            <Button 
                type="submit" 
                isLoading={loading} 
                leftIcon={<Save size={20} className="stroke-[2.5]" />}
                className="px-8 py-3 bg-gradient-to-r from-indigo-600 to-indigo-700 hover:from-indigo-700 hover:to-indigo-800 text-white rounded-xl shadow-xl shadow-indigo-600/20 hover:shadow-indigo-600/30 transition-all duration-300 transform hover:-translate-y-1 active:translate-y-0 border-none"
            >
              <span className="text-base font-extrabold">{t('common.save')}</span>
            </Button>
          </div>
        </form>
      </div>
    </section>
  );
};

export default ProfileForm;
