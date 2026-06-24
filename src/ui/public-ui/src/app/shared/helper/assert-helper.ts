import { MultiLanguage } from "@I14Y-ch/bfs-iop-admin-web-api-client";

export const hasAnyLangText = (ml?: MultiLanguage) =>
  !!ml && Object.values(ml).some(v => v?.trim());