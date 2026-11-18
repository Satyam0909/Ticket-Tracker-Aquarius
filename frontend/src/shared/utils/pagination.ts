export interface PaginationRange {
  first: number
  last: number
  pageCount: number
}

export function getPaginationRange(
  page: number,
  pageSize: number,
  totalCount: number,
): PaginationRange {
  return {
    first: totalCount === 0 ? 0 : (page - 1) * pageSize + 1,
    last: Math.min(page * pageSize, totalCount),
    pageCount: Math.max(1, Math.ceil(totalCount / pageSize)),
  }
}
