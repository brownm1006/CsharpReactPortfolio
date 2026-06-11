type RequiredField<TFormData, TLabel extends string> = [keyof TFormData, TLabel];

export function getMissingRequiredFields<TFormData extends Record<string, string>, TLabel extends string>(
  requiredFields: RequiredField<TFormData, TLabel>[],
  formData: TFormData,
): TLabel[] {
  return requiredFields
    .filter(([fieldName]) => !formData[fieldName])
    .map(([, label]) => label);
}
