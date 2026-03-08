import { useState, useEffect } from 'react'
import { api } from '../api/client'
import { useTranslations } from '../hooks/useTranslations'
import type { Category } from '../types'

export default function Categories() {
  const { t } = useTranslations()
  const [list, setList] = useState<Category[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [name, setName] = useState('')
  const [editingId, setEditingId] = useState<string | null>(null)
  const [editName, setEditName] = useState('')

  const load = async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await api.categories.list()
      setList(data)
    } catch (e) {
      setError(e instanceof Error ? e.message : t('categories_failed_load'))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps -- run once on mount
  }, [])

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    const trimmed = name.trim()
    if (!trimmed) return
    setError(null)
    try {
      await api.categories.create({ name: trimmed })
      setName('')
      await load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to create')
    }
  }

  const startEdit = (c: Category) => {
    setEditingId(c.id)
    setEditName(c.name)
  }

  const cancelEdit = () => {
    setEditingId(null)
    setEditName('')
  }

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!editingId) return
    const trimmed = editName.trim()
    if (!trimmed) return
    setError(null)
    try {
      await api.categories.update(editingId, { name: trimmed })
      cancelEdit()
      await load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to update')
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm(t('categories_delete_confirm'))) return
    setError(null)
    try {
      await api.categories.delete(id)
      if (editingId === id) cancelEdit()
      await load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Failed to delete')
    }
  }

  return (
    <>
      <h1 className="page-title">{t('categories_title')}</h1>

      <div className="card">
        <form onSubmit={handleCreate}>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="cat-name">{t('categories_add')}</label>
              <input
                id="cat-name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder={t('categories_placeholder')}
              />
            </div>
            <button type="submit" className="btn btn-primary">{t('common_add')}</button>
          </div>
        </form>
      </div>

      {error && <p className="error-msg">{error}</p>}

      <div className="card">
        {loading ? (
          <p className="empty">{t('common_loading')}</p>
        ) : list.length === 0 ? (
          <p className="empty">{t('categories_no_categories')}</p>
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>{t('common_name')}</th>
                  <th style={{ width: 120 }}>{t('common_actions')}</th>
                </tr>
              </thead>
              <tbody>
                {list.map((c) => (
                  <tr key={c.id}>
                    <td data-label={t('common_name')}>
                      {editingId === c.id ? (
                        <form onSubmit={handleUpdate} style={{ display: 'inline-flex', gap: '0.5rem', alignItems: 'center', flexWrap: 'wrap' }}>
                          <input
                            value={editName}
                            onChange={(e) => setEditName(e.target.value)}
                            autoFocus
                            style={{ width: 180 }}
                          />
                          <button type="submit" className="btn btn-primary btn-sm">{t('common_save')}</button>
                          <button type="button" className="btn btn-secondary btn-sm" onClick={cancelEdit}>{t('common_cancel')}</button>
                        </form>
                      ) : (
                        c.name
                      )}
                    </td>
                    <td className="actions-cell" data-label={t('common_actions')}>
                      {editingId !== c.id && (
                        <div className="actions">
                          <button type="button" className="btn btn-secondary btn-sm" onClick={() => startEdit(c)}>{t('common_edit')}</button>
                          <button type="button" className="btn btn-danger btn-sm" onClick={() => handleDelete(c.id)}>{t('common_delete')}</button>
                        </div>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </>
  )
}
