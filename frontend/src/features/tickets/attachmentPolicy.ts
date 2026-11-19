export const maxAttachmentsPerTicket = 3
export const maxAttachmentSizeBytes = 10 * 1024 * 1024

export const blockedAttachmentExtensions = [
  '.html',
  '.htm',
  '.js',
  '.mjs',
  '.cjs',
  '.php',
  '.exe',
  '.sh',
  '.bat',
]

export const allowedAttachmentExtensions = [
  '.jpg',
  '.jpeg',
  '.png',
  '.tiff',
  '.pdf',
  '.msg',
  '.doc',
  '.docx',
  '.txt',
  '.csv',
  '.xls',
  '.xlsx',
  '.xlsm',
  '.xltx',
  '.xltm',
  '.ppt',
  '.pptx',
  '.zip',
  '.rar',
  '.igs',
  '.stp',
  '.step',
  '.x_t',
  '.catpart',
  '.dwg',
  '.stl',
  '.sla',
  '.dxf',
  '.jt',
]

export const attachmentAccept = allowedAttachmentExtensions.join(',')

export function validateAttachmentFiles(files: File[], existingCount = 0): string | null {
  if (existingCount + files.length > maxAttachmentsPerTicket) {
    return `A ticket can have no more than ${maxAttachmentsPerTicket} attachments.`
  }

  for (const file of files) {
    if (file.size === 0) return 'Empty files cannot be uploaded.'
    if (file.size > maxAttachmentSizeBytes) return 'Each attachment must be 10 MB or smaller.'

    const extension = file.name.slice(file.name.lastIndexOf('.')).toLowerCase()
    if (
      blockedAttachmentExtensions.includes(extension) ||
      !allowedAttachmentExtensions.includes(extension)
    ) {
      return 'This file type is not allowed. Choose one of the listed supported formats.'
    }
  }

  return null
}
