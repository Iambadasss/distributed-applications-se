import {
  Banknote,
  Bus,
  CircleCheck,
  Clapperboard,
  CreditCard,
  Dumbbell,
  Edit3,
  HeartPulse,
  House,
  Music2,
  PiggyBank,
  Plane,
  Plus,
  Popcorn,
  RotateCcw,
  Save,
  Search,
  Trash2,
  Utensils,
  Wallet,
  X
} from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { api, cleanParams, getErrorMessage } from '../api/client';

const defaultPageSize = 10;
const iconMap = {
  wallet: Wallet,
  salary: Banknote,
  food: Utensils,
  home: House,
  transport: Bus,
  health: HeartPulse,
  entertainment: Popcorn,
  spotify: Music2,
  netflix: Clapperboard,
  gym: Dumbbell,
  travel: Plane,
  savings: PiggyBank
};

export function ResourcePage({ config }) {
  const [items, setItems] = useState([]);
  const [pagination, setPagination] = useState({ page: 1, pageSize: defaultPageSize, totalPages: 1, totalItems: 0 });
  const [filters, setFilters] = useState(() => createEmptyFilters(config));
  const [sortBy, setSortBy] = useState(config.defaultSort);
  const [sortDirection, setSortDirection] = useState(config.defaultDirection);
  const [categories, setCategories] = useState([]);
  const [editing, setEditing] = useState(null);
  const [editorOpen, setEditorOpen] = useState(false);
  const [form, setForm] = useState(() => createEmptyForm(config));
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);
  const [chargingId, setChargingId] = useState(null);

  const categoryOptions = useMemo(() => categories.map((category) => ({
    value: category.id,
    type: category.categoryType,
    label: `${category.name} (${getCategoryTypeLabel(category.categoryType)})`
  })).filter((category) => !config.categoryTypes || config.categoryTypes.includes(category.type)), [categories, config.categoryTypes]);

  const loadCategories = useCallback(async () => {
    if (!config.needsCategories) {
      return;
    }

    const response = await api.get('/categories', {
      params: { page: 1, pageSize: 100, sortBy: 'name', sortDirection: 'asc', isActive: true }
    });
    setCategories(response.data.items || []);
  }, [config.needsCategories]);

  const loadItems = useCallback(async (nextPage = pagination.page) => {
    setLoading(true);
    setError('');

    try {
      const response = await api.get(config.endpoint, {
        params: cleanParams({
          ...filters,
          page: nextPage,
          pageSize: pagination.pageSize,
          sortBy,
          sortDirection
        })
      });

      setItems(response.data.items || []);
      setPagination(response.data.pagination || { page: nextPage, pageSize: pagination.pageSize, totalPages: 1, totalItems: 0 });
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setLoading(false);
    }
  }, [config.endpoint, filters, pagination.page, pagination.pageSize, sortBy, sortDirection]);

  useEffect(() => {
    loadCategories().catch((requestError) => setError(getErrorMessage(requestError)));
  }, [loadCategories]);

  useEffect(() => {
    setItems([]);
    setPagination({ page: 1, pageSize: defaultPageSize, totalPages: 1, totalItems: 0 });
    setFilters(createEmptyFilters(config));
    setSortBy(config.defaultSort);
    setSortDirection(config.defaultDirection);
    setEditing(null);
    setEditorOpen(false);
    setForm(createEmptyForm(config));
    setError('');
    setMessage('');
  }, [config]);

  useEffect(() => {
    if (!message) {
      return undefined;
    }

    const timer = window.setTimeout(() => setMessage(''), 3500);
    return () => window.clearTimeout(timer);
  }, [message]);

  useEffect(() => {
    loadItems(1);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [config, sortBy, sortDirection]);

  function updateFilter(event) {
    const { name, value } = event.target;
    setFilters((current) => ({ ...current, [name]: value }));
  }

  function updateForm(event) {
    const { name, value, type, checked } = event.target;
    setForm((current) => {
      const next = { ...current, [name]: type === 'checkbox' ? checked : value };

      if (name === 'cycle' && value !== 'custom') {
        next.customCycleDays = '';
      }

      return next;
    });
  }

  function startCreate() {
    setEditing(null);
    setForm(createEmptyForm(config));
    setEditorOpen(true);
    setMessage('');
    setError('');
  }

  function startEdit(item) {
    setEditing(item);
    setForm(createFormFromItem(config, item));
    setEditorOpen(true);
    setMessage('');
    setError('');
  }

  function closeEditor() {
    setEditing(null);
    setEditorOpen(false);
    setForm(createEmptyForm(config));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setError('');
    setMessage('');

    try {
      let payload = buildPayload(config, form);
      const localError = getLocalValidationError(config, payload, categoryOptions);
      if (localError) {
        setError(localError);
        return;
      }

      payload = applyResourceRules(config, payload, categoryOptions);

      if (config.key === 'categories') {
        payload = {
          ...payload,
          isActive: editing ? Boolean(editing.isActive) : true
        };
      }

      if (editing) {
        await api.put(`${config.endpoint}/${editing.id}`, payload);
        setMessage('Записът е обновен.');
      } else {
        await api.post(config.endpoint, payload);
        setMessage('Записът е създаден.');
      }

      setEditing(null);
      setEditorOpen(false);
      setForm(createEmptyForm(config));
      await loadItems(editing ? pagination.page : 1);
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  }

  async function handleDelete(item) {
    const confirmed = window.confirm('Сигурен ли си, че искаш да изтриеш този запис?');
    if (!confirmed) {
      return;
    }

    try {
      await api.delete(`${config.endpoint}/${item.id}`);
      setMessage('Записът е изтрит.');
      await loadItems(pagination.page);
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  }

  async function handleActivateCategory(item) {
    const confirmed = window.confirm(`Да активирам ли категорията "${item.name}"?`);
    if (!confirmed) {
      return;
    }

    setError('');
    setMessage('');

    try {
      await api.put(`${config.endpoint}/${item.id}`, buildCategoryUpdatePayload(item, true));
      setMessage('Категорията е активирана.');
      await loadItems(pagination.page);
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  }

  async function handleSubscriptionCharge(item) {
    if (item.status !== 'active') {
      setError('Само активен абонамент може да бъде таксуван.');
      setMessage('');
      return;
    }

    const confirmed = window.confirm(`Да запиша ли таксата за "${item.name}"?`);
    if (!confirmed) {
      return;
    }

    setChargingId(item.id);
    setError('');
    setMessage('');

    try {
      const nextDueDate = calculateNextSubscriptionDate(item);

      await api.post('/financial-records', {
        categoryId: item.categoryId,
        recordType: 'expense',
        amount: item.amount,
        currency: item.currency,
        recordDate: item.nextDueDate,
        description: item.name,
        note: 'Такса от абонамент',
        isRecurring: true
      });

      await api.put(`${config.endpoint}/${item.id}`, buildSubscriptionUpdatePayload(item, nextDueDate));
      setMessage(`Таксата за "${item.name}" е записана.`);
      await loadItems(pagination.page);
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setChargingId(null);
    }
  }

  function handleFilterSubmit(event) {
    event.preventDefault();
    loadItems(1);
  }

  function resetFilters() {
    setFilters(createEmptyFilters(config));
    setTimeout(() => loadItems(1), 0);
  }

  return (
    <section className="page-section">
      <div className="section-heading">
        <div>
          <h2>{config.title}</h2>
          {config.description && <p className="section-description">{config.description}</p>}
        </div>
        <button className="primary-button new-record-button" type="button" onClick={startCreate}>
          <Plus aria-hidden="true" />
          Нов запис
        </button>
      </div>

      <form className="filter-band" onSubmit={handleFilterSubmit}>
        {config.filters.filter((filter) => !filter.hidden).map((filter) => (
          <FilterControl key={filter.name} filter={filter} value={filters[filter.name] ?? ''} categories={categoryOptions} onChange={updateFilter} />
        ))}
        <label>
          <span>Сортиране</span>
          <select value={sortBy} onChange={(event) => setSortBy(event.target.value)}>
            {config.sortOptions.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
          </select>
        </label>
        <label>
          <span>Ред</span>
          <select value={sortDirection} onChange={(event) => setSortDirection(event.target.value)}>
            <option value="asc">Старо към ново</option>
            <option value="desc">Ново към старо</option>
          </select>
        </label>
        <div className="toolbar-buttons">
          <button className="icon-button filled" type="submit" title="Търси">
            <Search aria-hidden="true" />
          </button>
          <button className="icon-button" type="button" onClick={resetFilters} title="Изчисти">
            <RotateCcw aria-hidden="true" />
          </button>
        </div>
      </form>

      {error && <p className="error-message">{error}</p>}
      {message && <p className="success-message">{message}</p>}

      <div className="table-wrap">
        <table className="data-table">
          <thead>
            <tr>
              {config.columns.map((column) => <th key={column.key}>{column.label}</th>)}
              <th className="actions-col">Действия</th>
            </tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr key={item.id} className={getRowClassName(config, item)}>
                {config.columns.map((column) => (
                  <td key={column.key}>{formatValue(item, column)}</td>
                ))}
                <td className="row-actions">
                  {config.key === 'subscriptions' && (
                    <button className="icon-button" type="button" onClick={() => handleSubscriptionCharge(item)} disabled={chargingId === item.id} title="Запиши такса">
                      <CreditCard aria-hidden="true" />
                    </button>
                  )}
                  <button className="icon-button" type="button" onClick={() => startEdit(item)} title="Редактирай">
                    <Edit3 aria-hidden="true" />
                  </button>
                  {config.key === 'categories' && item.isActive === false ? (
                    <button className="icon-button activate-icon" type="button" onClick={() => handleActivateCategory(item)} title="Активирай">
                      <CircleCheck aria-hidden="true" />
                    </button>
                  ) : (
                    <button className="icon-button danger-icon" type="button" onClick={() => handleDelete(item)} title="Изтрий">
                      <Trash2 aria-hidden="true" />
                    </button>
                  )}
                </td>
              </tr>
            ))}
            {!items.length && (
              <tr>
                <td colSpan={config.columns.length + 1}>{loading ? 'Зареждане...' : 'Няма записи.'}</td>
              </tr>
            )}
          </tbody>
        </table>

        <div className="pagination">
          <span>Страница {pagination.page || 1} от {pagination.totalPages || 1}</span>
          <div>
            <button type="button" onClick={() => loadItems(Math.max(1, pagination.page - 1))} disabled={!pagination.hasPreviousPage}>Назад</button>
            <button type="button" onClick={() => loadItems(Math.min(pagination.totalPages, pagination.page + 1))} disabled={!pagination.hasNextPage}>Напред</button>
          </div>
        </div>
      </div>

      {editorOpen && (
        <div className="modal-backdrop" role="presentation">
          <form className="editor-form modal-card" onSubmit={handleSubmit}>
            <div className="editor-title">
              <h3>{editing ? 'Редакция' : 'Нов запис'}</h3>
              <button className="icon-button" type="button" onClick={closeEditor} title="Затвори">
                <X aria-hidden="true" />
              </button>
            </div>

            {config.fields.filter((field) => shouldShowField(field, Boolean(editing), form)).map((field) => (
              <FieldControl key={field.name} field={field} value={form[field.name]} categories={categoryOptions} onChange={updateForm} />
            ))}

            <div className="form-actions">
              <button className="secondary-button" type="button" onClick={closeEditor}>
                Отказ
              </button>
              <button className="primary-button" type="submit">
                <Save aria-hidden="true" />
                {editing ? 'Запази' : 'Създай'}
              </button>
            </div>
          </form>
        </div>
      )}
    </section>
  );
}

function FilterControl({ filter, value, categories, onChange }) {
  if (filter.type === 'select') {
    return (
      <label>
        <span>{filter.label}</span>
        <select name={filter.name} value={value} onChange={onChange}>
          <option value="">Всички</option>
          {filter.options.map((option) => <option key={getOptionValue(option)} value={getOptionValue(option)}>{getOptionLabel(option)}</option>)}
        </select>
      </label>
    );
  }

  if (filter.type === 'booleanSelect') {
    return (
      <label>
        <span>{filter.label}</span>
        <select name={filter.name} value={value} onChange={onChange}>
          <option value="">Всички</option>
          <option value="true">{filter.trueLabel || 'Да'}</option>
          <option value="false">{filter.falseLabel || 'Не'}</option>
        </select>
      </label>
    );
  }

  if (filter.type === 'category') {
    return (
      <label>
        <span>{filter.label}</span>
        <select name={filter.name} value={value} onChange={onChange}>
          <option value="">Всички</option>
          {categories.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
        </select>
      </label>
    );
  }

  return (
    <label>
      <span>{filter.label}</span>
      <input
        name={filter.name}
        type={filter.type === 'search' ? 'search' : filter.type}
        min={filter.min}
        max={filter.max}
        value={value}
        onChange={onChange}
      />
    </label>
  );
}

function FieldControl({ field, value, categories, onChange }) {
  if (field.type === 'select') {
    return (
      <label>
        <span>{field.label}</span>
        <select name={field.name} value={value ?? ''} onChange={onChange} required={field.required}>
          <option value="" disabled={field.required}>{field.required ? 'Избери' : 'Без избор'}</option>
          {field.options.map((option) => <option key={getOptionValue(option)} value={getOptionValue(option)}>{getOptionLabel(option)}</option>)}
        </select>
        {field.name === 'iconName' && value && (
          <IconPreview value={value} options={field.options} />
        )}
        {field.helper && <small className="field-helper">{field.helper}</small>}
      </label>
    );
  }

  if (field.type === 'category') {
    return (
      <label>
        <span>{field.label}</span>
        <select name={field.name} value={value ?? ''} onChange={onChange} required={field.required}>
          <option value="" disabled={field.required}>Избери</option>
          {categories.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}
        </select>
        {field.helper && <small className="field-helper">{field.helper}</small>}
      </label>
    );
  }

  if (field.type === 'textarea') {
    return (
      <label>
        <span>{field.label}</span>
        <textarea name={field.name} value={value ?? ''} onChange={onChange} maxLength={field.maxLength}></textarea>
        {field.helper && <small className="field-helper">{field.helper}</small>}
      </label>
    );
  }

  if (field.type === 'checkbox') {
    return (
      <label className="checkbox-row">
        <input name={field.name} type="checkbox" checked={Boolean(value)} onChange={onChange} />
        <span>{field.label}</span>
        {field.helper && <small className="field-helper">{field.helper}</small>}
      </label>
    );
  }

  if (field.type === 'colorText') {
    return (
      <label>
        <span>{field.label}</span>
        <div className="color-field">
          <input name={field.name} type="color" value={value || '#3A86FF'} onChange={onChange} />
          <input
            name={field.name}
            value={value || ''}
            onChange={onChange}
            pattern={field.pattern}
            maxLength={field.maxLength}
            placeholder="#3A86FF"
          />
        </div>
        {field.helper && <small className="field-helper">{field.helper}</small>}
      </label>
    );
  }

  return (
    <label>
      <span>{field.label}</span>
      <input
        name={field.name}
        type={field.type}
        value={value ?? ''}
        onChange={onChange}
        required={field.required}
        minLength={field.minLength}
        maxLength={field.maxLength}
        min={field.min}
        max={field.max}
        step={field.step}
        pattern={field.pattern}
      />
      {field.helper && <small className="field-helper">{field.helper}</small>}
    </label>
  );
}

function createEmptyFilters(config) {
  return Object.fromEntries(config.filters.map((filter) => [filter.name, filter.defaultValue ?? '']));
}

function createEmptyForm(config) {
  return Object.fromEntries(config.fields.map((field) => [field.name, resolveDefaultValue(field)]));
}

function createFormFromItem(config, item) {
  return Object.fromEntries(config.fields.map((field) => [field.name, item[field.name] ?? field.defaultValue ?? (field.type === 'checkbox' ? false : '')]));
}

function resolveDefaultValue(field) {
  if (field.defaultValue === 'currentMonth') {
    return new Date().getMonth() + 1;
  }

  if (field.defaultValue === 'currentYear') {
    return new Date().getFullYear();
  }

  if (field.defaultValue === 'currentDate') {
    return formatDateOnlyFromDate(new Date());
  }

  return field.defaultValue ?? (field.type === 'checkbox' ? false : '');
}

function buildPayload(config, form) {
  const payload = {};

  for (const field of config.fields) {
    const value = form[field.name];
    if (value === '' && !field.required) {
      payload[field.name] = null;
      continue;
    }

    if (field.type === 'number' || field.type === 'category') {
      payload[field.name] = value === '' ? null : Number(value);
      continue;
    }

    payload[field.name] = value;
  }

  return payload;
}

function buildCategoryUpdatePayload(item, isActive) {
  return {
    name: item.name,
    categoryType: item.categoryType,
    description: item.description,
    colorHex: item.colorHex,
    iconName: item.iconName,
    isActive
  };
}

function shouldShowField(field, isEditing, form) {
  if (field.showOnCreate === false && !isEditing) {
    return false;
  }

  if (field.showOnEdit === false && isEditing) {
    return false;
  }

  if (field.showWhen && form[field.showWhen.name] !== field.showWhen.value) {
    return false;
  }

  return true;
}

function getLocalValidationError(config, payload, categoryOptions) {
  if (!payload.categoryId) {
    return '';
  }

  const category = categoryOptions.find((option) => Number(option.value) === Number(payload.categoryId));
  if (!category) {
    return 'Избраната категория не е валидна за тази страница.';
  }

  if (config.key === 'financial-records' && !['income', 'expense', 'subscription'].includes(category.type)) {
    return 'Финансов запис може да използва само приходна, разходна или абонаментна категория.';
  }

  if (config.key === 'subscriptions' && category.type !== 'subscription') {
    return 'Абонамент може да използва само абонаментна категория.';
  }

  if (config.key === 'budgets' && category.type !== 'expense') {
    return 'Бюджетът се създава за разходна категория.';
  }

  return '';
}

function applyResourceRules(config, payload, categoryOptions) {
  if (config.key === 'financial-records') {
    const category = categoryOptions.find((option) => Number(option.value) === Number(payload.categoryId));
    return {
      ...payload,
      recordType: category?.type === 'income' ? 'income' : 'expense',
      isRecurring: false
    };
  }

  if (config.key === 'subscriptions') {
    const customCycleDays = payload.cycle === 'custom' ? payload.customCycleDays : null;

    return {
      ...payload,
      customCycleDays,
      nextDueDate: calculateNextDueDateFromStart(payload.startDate, payload.cycle, customCycleDays)
    };
  }

  if (config.key === 'budgets') {
    return {
      ...payload,
      isActive: true
    };
  }

  return payload;
}

function buildSubscriptionUpdatePayload(item, nextDueDate) {
  return {
    categoryId: item.categoryId,
    name: item.name,
    amount: item.amount,
    currency: item.currency,
    cycle: item.cycle,
    customCycleDays: item.cycle === 'custom' ? item.customCycleDays : null,
    startDate: item.startDate,
    nextDueDate,
    status: item.status,
    description: item.description,
    includeInMonthlyForecast: Boolean(item.includeInMonthlyForecast)
  };
}

function calculateNextSubscriptionDate(item) {
  const currentDate = item.nextDueDate || item.startDate;

  if (item.cycle === 'weekly') {
    return addDaysToDate(currentDate, 7);
  }

  if (item.cycle === 'quarterly') {
    return addMonthsToDate(currentDate, 3);
  }

  if (item.cycle === 'yearly') {
    return addMonthsToDate(currentDate, 12);
  }

  if (item.cycle === 'custom') {
    return addDaysToDate(currentDate, Number(item.customCycleDays || 1));
  }

  return addMonthsToDate(currentDate, 1);
}

function calculateNextDueDateFromStart(startDate, cycle, customCycleDays) {
  if (cycle === 'weekly') {
    return addDaysToDate(startDate, 7);
  }

  if (cycle === 'quarterly') {
    return addMonthsToDate(startDate, 3);
  }

  if (cycle === 'yearly') {
    return addMonthsToDate(startDate, 12);
  }

  if (cycle === 'custom') {
    return addDaysToDate(startDate, Number(customCycleDays || 1));
  }

  return addMonthsToDate(startDate, 1);
}

function addMonthsToDate(value, monthsToAdd) {
  const { year, month, day } = parseDateOnly(value);
  const nextMonthIndex = month - 1 + monthsToAdd;
  const nextYear = year + Math.floor(nextMonthIndex / 12);
  const nextMonth = (nextMonthIndex % 12) + 1;
  const nextDay = Math.min(day, getDaysInMonth(nextYear, nextMonth));

  return formatDateOnly(nextYear, nextMonth, nextDay);
}

function addDaysToDate(value, daysToAdd) {
  const { year, month, day } = parseDateOnly(value);
  const date = new Date(Date.UTC(year, month - 1, day));
  date.setUTCDate(date.getUTCDate() + daysToAdd);

  return formatDateOnly(date.getUTCFullYear(), date.getUTCMonth() + 1, date.getUTCDate());
}

function parseDateOnly(value) {
  const [year, month, day] = String(value).split('-').map(Number);
  return { year, month, day };
}

function formatDateOnly(year, month, day) {
  return `${year}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
}

function formatDateOnlyFromDate(date) {
  return formatDateOnly(date.getFullYear(), date.getMonth() + 1, date.getDate());
}

function getDaysInMonth(year, month) {
  return new Date(Date.UTC(year, month, 0)).getUTCDate();
}

function getRowClassName(config, item) {
  if (config.key === 'budgets' && Number(item.remainingAmount || 0) < 0) {
    return 'warning-row';
  }

  return '';
}

function formatValue(item, column) {
  const value = item[column.key];

  if (column.type === 'icon') {
    const Icon = iconMap[value] || Wallet;
    const label = getOptionLabel(column.options?.find((option) => getOptionValue(option) === value) || value);

    return (
      <span className="table-icon-cell" title={label}>
        <Icon aria-label={label} />
      </span>
    );
  }

  if (column.options) {
    return getOptionLabel(column.options.find((option) => getOptionValue(option) === value) || value);
  }

  if (column.type === 'boolean') {
    return value ? 'Да' : 'Не';
  }

  if (column.type === 'activeStatus') {
    return value ? 'Активна' : 'Неактивна';
  }

  if (column.type === 'money') {
    return Number(value || 0).toFixed(2);
  }

  if (column.type === 'budgetRemaining') {
    const amount = Number(value || 0);
    const currency = item.currency || '';

    if (amount < 0) {
      return <span className="budget-status over">Над лимита с {Math.abs(amount).toFixed(2)} {currency}</span>;
    }

    if (amount === 0) {
      return <span className="budget-status zero">Лимитът е изчерпан</span>;
    }

    return <span className="budget-status ok">Остава {amount.toFixed(2)} {currency}</span>;
  }

  if (column.type === 'percent') {
    return `${Number(value || 0).toFixed(2)}%`;
  }

  if (column.type === 'dateTime') {
    return value ? new Date(value).toLocaleString('bg-BG') : '';
  }

  if (column.type === 'color') {
    return (
      <span className="color-chip">
        <span style={{ backgroundColor: value || '#d4d8dd' }}></span>
        {value || '-'}
      </span>
    );
  }

  return value ?? '';
}

function getOptionValue(option) {
  return option && typeof option === 'object' ? option.value : option;
}

function getOptionLabel(option) {
  if (option === null || option === undefined || option === '') {
    return '-';
  }

  return typeof option === 'object' ? option.label : option;
}

function getCategoryTypeLabel(value) {
  const labels = {
    income: 'Приход',
    expense: 'Разход',
    subscription: 'Абонамент',
    saving: 'Спестяване'
  };

  return labels[value] || value;
}

function IconPreview({ value, options }) {
  const Icon = iconMap[value] || Wallet;
  const label = getOptionLabel(options?.find((option) => getOptionValue(option) === value) || value);

  return (
    <span className="icon-preview">
      <Icon aria-hidden="true" />
      {label}
    </span>
  );
}
