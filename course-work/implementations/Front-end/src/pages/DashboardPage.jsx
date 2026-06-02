import { BarChart3, CalendarDays, Info, Target } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { api, cleanParams, getErrorMessage } from '../api/client';
import { recordTypeOptions, savingsGoalStatusOptions } from '../config/resources';

function monthNow() {
  const now = new Date();
  return {
    month: now.getMonth() + 1,
    year: now.getFullYear(),
    fromDate: `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-01`,
    toDate: new Date(now.getFullYear(), now.getMonth() + 1, 0).toISOString().slice(0, 10)
  };
}

export function DashboardPage() {
  const defaults = useMemo(() => monthNow(), []);
  const [summaryFilters, setSummaryFilters] = useState({ month: defaults.month, year: defaults.year });
  const [categoryFilters, setCategoryFilters] = useState({
    fromDate: defaults.fromDate,
    toDate: defaults.toDate,
    recordType: ''
  });
  const [savingsFilters, setSavingsFilters] = useState({ status: '', targetDateTo: '' });
  const [summary, setSummary] = useState(null);
  const [categories, setCategories] = useState([]);
  const [goals, setGoals] = useState([]);
  const [error, setError] = useState('');

  async function loadReports() {
    setError('');
    try {
      const [summaryResponse, categoryResponse, savingsResponse] = await Promise.all([
        api.get('/reports/monthly-summary', { params: cleanParams(summaryFilters) }),
        api.get('/reports/category-summary', { params: cleanParams(categoryFilters) }),
        api.get('/reports/savings-progress', { params: cleanParams({ ...savingsFilters, pageSize: 6 }) })
      ]);

      setSummary(summaryResponse.data);
      setCategories(categoryResponse.data);
      setGoals(savingsResponse.data.items || []);
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  }

  useEffect(() => {
    loadReports();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function updateSummary(event) {
    const { name, value } = event.target;
    setSummaryFilters((current) => ({ ...current, [name]: value }));
  }

  function updateCategory(event) {
    const { name, value } = event.target;
    setCategoryFilters((current) => ({ ...current, [name]: value }));
  }

  function updateSavings(event) {
    const { name, value } = event.target;
    setSavingsFilters((current) => ({ ...current, [name]: value }));
  }

  return (
    <section className="page-section">
      <div className="section-heading">
        <div>
          <h2>Табло</h2>
        </div>
      </div>

      {error && <p className="error-message">{error}</p>}

      <div className="dashboard-grid">
        <div className="dashboard-intro">
          <Info aria-hidden="true" />
          <span>Избери период, прегледай обобщението и виж кои категории или цели имат нужда от внимание.</span>
        </div>

        <div className="report-panel summary-panel">
          <div className="panel-heading">
            <CalendarDays aria-hidden="true" />
            <div>
              <h3>Месечно обобщение</h3>
              <span className="panel-note">Суми за избрания месец</span>
            </div>
          </div>
          <form className="filter-band embedded-filter" onSubmit={(event) => { event.preventDefault(); loadReports(); }}>
            <label>
              <span>Месец</span>
              <input name="month" type="number" min="1" max="12" value={summaryFilters.month} onChange={updateSummary} />
            </label>
            <label>
              <span>Година</span>
              <input name="year" type="number" min="2000" max="2100" value={summaryFilters.year} onChange={updateSummary} />
            </label>
            <button className="primary-button apply-button" type="submit">
              <CalendarDays aria-hidden="true" />
              Покажи
            </button>
          </form>
        </div>

        <div className="metric-row">
          <Metric label="Приходи" description="Всички записи от тип приход." value={summary?.totalIncome} currency={summary?.currency} />
          <Metric label="Разходи" description="Всички записи от тип разход." value={summary?.totalExpenses} currency={summary?.currency} />
          <Metric label="Свободни" description="Приходи минус разходи." value={Number(summary?.totalIncome || 0) - Number(summary?.totalExpenses || 0)} currency={summary?.currency} />
        </div>

        <div className="report-panel">
          <div className="panel-heading">
            <BarChart3 aria-hidden="true" />
            <div>
              <h3>Разпределение по категории</h3>
              <span className="panel-note">Групира приходите и разходите по категория</span>
            </div>
          </div>
          <div className="inline-filters">
            <label>
              <span>От дата</span>
              <input name="fromDate" type="date" value={categoryFilters.fromDate} onChange={updateCategory} />
            </label>
            <label>
              <span>До дата</span>
              <input name="toDate" type="date" value={categoryFilters.toDate} onChange={updateCategory} />
            </label>
            <label>
              <span>Тип запис</span>
              <select name="recordType" value={categoryFilters.recordType} onChange={updateCategory}>
                <option value="">Всички</option>
                {recordTypeOptions.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
              </select>
            </label>
            <button className="secondary-button apply-button" type="button" onClick={loadReports}>
              Приложи
            </button>
          </div>
          <div className="compact-table-wrap">
            <table className="data-table compact">
              <thead>
                <tr>
                  <th>Категория</th>
                  <th>Тип</th>
                  <th>Сума</th>
                  <th>Брой</th>
                </tr>
              </thead>
              <tbody>
                {categories.map((item) => (
                  <tr key={`${item.categoryId}-${item.recordType}-${item.currency}`}>
                    <td>{item.categoryName}</td>
                    <td>{recordTypeOptions.find((option) => option.value === item.recordType)?.label || item.recordType}</td>
                    <td>{Number(item.totalAmount).toFixed(2)} {item.currency}</td>
                    <td>{item.recordsCount}</td>
                  </tr>
                ))}
                {!categories.length && <tr><td colSpan="4">Няма данни за избрания период. Добави финансови записи или промени датите.</td></tr>}
              </tbody>
            </table>
          </div>
        </div>

        <div className="report-panel">
          <div className="panel-heading">
            <Target aria-hidden="true" />
            <div>
              <h3>Напредък по спестовни цели</h3>
              <span className="panel-note">Следи прогреса спрямо целевата сума</span>
            </div>
          </div>
          <div className="inline-filters">
            <label>
              <span>Статус</span>
              <select name="status" value={savingsFilters.status} onChange={updateSavings}>
                <option value="">Всички</option>
                {savingsGoalStatusOptions.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
              </select>
            </label>
            <label>
              <span>Краен срок до</span>
              <input name="targetDateTo" type="date" value={savingsFilters.targetDateTo} onChange={updateSavings} />
            </label>
            <button className="secondary-button apply-button" type="button" onClick={loadReports}>
              Приложи
            </button>
          </div>
          <div className="goal-list">
            {goals.map((goal) => (
              <div className="goal-row" key={goal.id}>
                <div>
                  <strong>{goal.name}</strong>
                  <span>{goal.currentAmount} / {goal.targetAmount} {goal.currency}</span>
                </div>
                <progress max="100" value={goal.progressPercent}></progress>
              </div>
            ))}
            {!goals.length && <p className="muted">Няма цели за избраните филтри. Добави спестовна цел от страницата „Цели“.</p>}
          </div>
        </div>
      </div>
    </section>
  );
}

function Metric({ label, description, value, currency }) {
  return (
    <div className="metric">
      <span>{label}</span>
      <strong>{Number(value || 0).toFixed(2)}</strong>
      <small>{currency || 'BGN'}</small>
      <p>{description}</p>
    </div>
  );
}
