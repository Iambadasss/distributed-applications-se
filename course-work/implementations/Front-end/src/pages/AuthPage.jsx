import { PiggyBank } from 'lucide-react';
import { useState } from 'react';
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom';
import { getErrorMessage } from '../api/client';
import { currencyOptions } from '../config/resources';
import { useAuth } from '../state/AuthContext.jsx';

const emptyLogin = {
  email: '',
  password: ''
};

const emptyRegister = {
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  defaultCurrency: 'BGN'
};

export function AuthPage({ mode }) {
  const isRegister = mode === 'register';
  const [form, setForm] = useState(isRegister ? emptyRegister : emptyLogin);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);
  const { login, register, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const destination = location.state?.from?.pathname || '/';

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  function updateField(event) {
    const { name, value } = event.target;
    setForm((current) => ({ ...current, [name]: value }));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setError('');
    setSaving(true);

    try {
      if (isRegister) {
        await register(form);
      } else {
        await login(form);
      }
      navigate(destination, { replace: true });
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setSaving(false);
    }
  }

  return (
    <main className="auth-page">
      <section className="auth-panel">
        <div className="auth-brand">
          <PiggyBank aria-hidden="true" />
          <div>
            <h1>Personal Finance Tracker</h1>
            <span>{isRegister ? 'Регистрация' : 'Вход'}</span>
          </div>
        </div>

        <form className="auth-form" onSubmit={handleSubmit}>
          {isRegister && (
            <div className="two-column">
              <label>
                <span>Име</span>
                <input
                  name="firstName"
                  value={form.firstName}
                  onChange={updateField}
                  required
                  minLength={2}
                  maxLength={50}
                />
              </label>
              <label>
                <span>Фамилия</span>
                <input
                  name="lastName"
                  value={form.lastName}
                  onChange={updateField}
                  required
                  minLength={2}
                  maxLength={50}
                />
              </label>
            </div>
          )}

          <label>
            <span>Имейл</span>
            <input
              name="email"
              type="email"
              value={form.email}
              onChange={updateField}
              required
              maxLength={100}
            />
          </label>

          <label>
            <span>Парола</span>
            <input
              name="password"
              type="password"
              value={form.password}
              onChange={updateField}
              required
              minLength={8}
              maxLength={100}
            />
          </label>

          {isRegister && (
            <label>
              <span>Валута</span>
              <select name="defaultCurrency" value={form.defaultCurrency} onChange={updateField}>
                {currencyOptions.map((option) => (
                  <option key={option} value={option}>{option}</option>
                ))}
              </select>
            </label>
          )}

          {error && <p className="error-message">{error}</p>}

          <button className="primary-button" type="submit" disabled={saving}>
            {saving ? 'Изчакване...' : isRegister ? 'Създай профил' : 'Вход'}
          </button>
        </form>

        <p className="auth-switch">
          {isRegister ? (
            <Link to="/login">Вече имам профил</Link>
          ) : (
            <Link to="/register">Създай нов профил</Link>
          )}
        </p>
      </section>
    </main>
  );
}
