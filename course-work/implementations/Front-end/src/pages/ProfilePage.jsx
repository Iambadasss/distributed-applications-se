import { Save, Trash2 } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api, getErrorMessage } from '../api/client';
import { currencyOptions } from '../config/resources';
import { useAuth } from '../state/AuthContext.jsx';

export function ProfilePage() {
  const { user, refreshUser, logout } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    defaultCurrency: 'BGN'
  });
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    if (user) {
      setForm({
        firstName: user.firstName || '',
        lastName: user.lastName || '',
        email: user.email || '',
        defaultCurrency: user.defaultCurrency || 'BGN'
      });
    }
  }, [user]);

  function updateField(event) {
    const { name, value } = event.target;
    setForm((current) => ({ ...current, [name]: value }));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setError('');
    setMessage('');

    try {
      await api.put(`/users/${user.id}`, { ...form, isActive: true });
      await refreshUser();
      setMessage('Профилът е обновен.');
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  }

  async function handleDelete() {
    const confirmed = window.confirm('Сигурен ли си, че искаш да деактивираш профила?');
    if (!confirmed) {
      return;
    }

    try {
      await api.delete(`/users/${user.id}`);
      logout();
      navigate('/login');
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  }

  return (
    <section className="page-section">
      <div className="section-heading">
        <div>
          <h2>Профил</h2>
        </div>
      </div>

      <form className="editor-form narrow-form" onSubmit={handleSubmit}>
        <div className="two-column">
          <label>
            <span>Име</span>
            <input name="firstName" value={form.firstName} onChange={updateField} required minLength={2} maxLength={50} />
          </label>
          <label>
            <span>Фамилия</span>
            <input name="lastName" value={form.lastName} onChange={updateField} required minLength={2} maxLength={50} />
          </label>
        </div>
        <label>
          <span>Имейл</span>
          <input name="email" type="email" value={form.email} onChange={updateField} required maxLength={100} />
        </label>
        <label>
          <span>Валута</span>
          <select name="defaultCurrency" value={form.defaultCurrency} onChange={updateField}>
            {currencyOptions.map((option) => <option key={option} value={option}>{option}</option>)}
          </select>
        </label>
        {message && <p className="success-message">{message}</p>}
        {error && <p className="error-message">{error}</p>}

        <div className="form-actions">
          <button className="primary-button" type="submit">
            <Save aria-hidden="true" />
            Запази
          </button>
          <button className="danger-button" type="button" onClick={handleDelete}>
            <Trash2 aria-hidden="true" />
            Деактивирай
          </button>
        </div>
      </form>
    </section>
  );
}
