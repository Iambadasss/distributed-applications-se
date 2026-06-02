import {
  LayoutDashboard,
  LogOut,
  PiggyBank,
  ReceiptText,
  Repeat,
  Tags,
  Target,
  UserRound,
  WalletCards
} from 'lucide-react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../state/AuthContext.jsx';

const navItems = [
  { to: '/', label: 'Табло', icon: LayoutDashboard },
  { to: '/categories', label: 'Категории', icon: Tags },
  { to: '/financial-records', label: 'Записи', icon: ReceiptText },
  { to: '/subscriptions', label: 'Абонаменти', icon: Repeat },
  { to: '/budgets', label: 'Бюджети', icon: WalletCards },
  { to: '/savings-goals', label: 'Цели', icon: Target }
];

export function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/login');
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <PiggyBank aria-hidden="true" />
          <div>
            <strong>Personal Finance</strong>
            <span>Tracker</span>
          </div>
        </div>

        <nav className="nav-list">
          {navItems.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink key={item.to} to={item.to} className="nav-link">
                <Icon aria-hidden="true" />
                <span>{item.label}</span>
              </NavLink>
            );
          })}
        </nav>

        <div className="sidebar-footer">
          <NavLink to="/profile" className="profile-link">
            <UserRound aria-hidden="true" />
            <span>{user?.firstName || 'Профил'}</span>
          </NavLink>
          <button className="icon-button" type="button" onClick={handleLogout} title="Изход">
            <LogOut aria-hidden="true" />
          </button>
        </div>
      </aside>

      <main className="main-content">
        <header className="topbar">
          <div>
            <h1>Personal Finance Tracker</h1>
          </div>
        </header>
        <Outlet />
      </main>
    </div>
  );
}
