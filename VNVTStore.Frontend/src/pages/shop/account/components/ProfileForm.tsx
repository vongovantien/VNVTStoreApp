import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Save } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { AvatarUpload } from '@/components/common';
import { createSchemas } from '@/utils/schemas';

export interface UpdateProfileData {
  fullName: string;
  email: string;
  phone: string;
  avatarUrl?: string;
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
  const [avatarUrl, setAvatarUrl] = useState<string | undefined>(initialData.avatar);

  const form = useForm({
    resolver: zodResolver(userBaseSchema),
    defaultValues: initialData
  });

  const [prevAvatar, setPrevAvatar] = useState(initialData.avatar);

  // Handle initialData changes (e.g. after fetch)
  if (initialData.avatar !== prevAvatar) {
    setPrevAvatar(initialData.avatar);
    setAvatarUrl(initialData.avatar);
  }

  useEffect(() => {
    form.reset(initialData);
  }, [initialData, form]);

  const handleSubmit = (data: { fullName: string; email: string; phone: string }) => {
    onSave({ ...data, avatarUrl });
  };

  return (
    <section className="bg-primary rounded-xl p-6 border shadow-sm border-secondary/10">
      <h2 className="text-xl font-bold mb-6 text-primary">{t('common.account.profile')}</h2>
      
      <div className="flex flex-col md:flex-row gap-8">
        <div className="flex flex-col items-center space-y-4">
          <AvatarUpload 
            currentAvatarUrl={avatarUrl}
            onUploadSuccess={setAvatarUrl}
            size="xl"
          />
          <p className="text-xs text-tertiary text-center max-w-[200px]">
             {t('common.account.avatarHint') || 'Hỗ trợ định dạng JPG, PNG, WebP. Tối đa 2MB.'}
          </p>
        </div>

        <form onSubmit={form.handleSubmit(handleSubmit)} className="grid grid-cols-1 md:grid-cols-2 gap-5 flex-1">
          <Input
            label={t('common.fields.fullName') || t('common.fields.name')}
            placeholder={t('common.placeholders.fullName')}
            {...form.register('fullName')}
            error={form.formState.errors.fullName?.message as string}
          />
          <Input
            label={t('common.fields.email')}
            type="email"
            {...form.register('email')}
            readOnly
            disabled
            className="bg-secondary/20 cursor-not-allowed"
            placeholder={t('common.placeholders.email')}
          />
          <Input
            label={t('common.fields.phone')}
            type="tel"
            placeholder={t('common.placeholders.phone')}
            {...form.register('phone')}
            error={form.formState.errors.phone?.message as string}
          />
          
          <div className="md:col-span-2 pt-4 flex justify-end">
            <Button type="submit" isLoading={loading} leftIcon={<Save size={18} />}>
              {t('common.save')}
            </Button>
          </div>
        </form>
      </div>
    </section>
  );
};

export default ProfileForm;
