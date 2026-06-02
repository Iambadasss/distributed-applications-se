export const currencyOptions = ['BGN', 'EUR', 'USD'];

export const categoryTypeOptions = [
  { value: 'income', label: 'Приходна категория' },
  { value: 'expense', label: 'Разходна категория' },
  { value: 'subscription', label: 'Абонаментна категория' }
];

export const recordTypeOptions = [
  { value: 'income', label: 'Приход' },
  { value: 'expense', label: 'Разход' }
];

export const subscriptionStatusOptions = [
  { value: 'active', label: 'Активен' },
  { value: 'paused', label: 'Пауза' },
  { value: 'cancelled', label: 'Прекратен' },
  { value: 'expired', label: 'Изтекъл' }
];

export const renewalCycleOptions = [
  { value: 'weekly', label: 'Седмично' },
  { value: 'monthly', label: 'Месечно' },
  { value: 'quarterly', label: 'Тримесечно' },
  { value: 'yearly', label: 'Годишно' },
  { value: 'custom', label: 'По избор' }
];

export const savingsGoalStatusOptions = [
  { value: 'active', label: 'Активна' },
  { value: 'completed', label: 'Завършена' },
  { value: 'cancelled', label: 'Отказана' }
];

export const iconOptions = [
  { value: 'wallet', label: 'Портфейл' },
  { value: 'salary', label: 'Заплата' },
  { value: 'food', label: 'Храна' },
  { value: 'home', label: 'Дом' },
  { value: 'transport', label: 'Транспорт' },
  { value: 'health', label: 'Здраве' },
  { value: 'entertainment', label: 'Забавление' },
  { value: 'spotify', label: 'Spotify' },
  { value: 'netflix', label: 'Netflix' },
  { value: 'gym', label: 'Фитнес' },
  { value: 'travel', label: 'Пътуване' },
  { value: 'savings', label: 'Спестявания' }
];

export const resources = [
  {
    key: 'categories',
    path: '/categories',
    title: 'Категории',
    description: 'Категориите са етикети за групиране на приходи, разходи и абонаменти. Абонаментната категория не е самият абонамент, а групата, към която абонаментът ще бъде записан.',
    endpoint: '/categories',
    defaultSort: 'name',
    defaultDirection: 'asc',
    columns: [
      { key: 'name', label: 'Име' },
      { key: 'categoryType', label: 'Тип', options: categoryTypeOptions },
      { key: 'iconName', label: 'Икона', type: 'icon', options: iconOptions },
      { key: 'colorHex', label: 'Цвят', type: 'color' },
      { key: 'isActive', label: 'Статус', type: 'activeStatus' },
      { key: 'createdAt', label: 'Създадена', type: 'dateTime' }
    ],
    filters: [
      { name: 'searchQuery', label: 'Търсене', type: 'search' },
      { name: 'categoryType', label: 'Тип', type: 'select', options: categoryTypeOptions },
      { name: 'isActive', label: 'Статус', type: 'booleanSelect', defaultValue: 'true', trueLabel: 'Активни', falseLabel: 'Неактивни' }
    ],
    fields: [
      { name: 'name', label: 'Име', type: 'text', required: true, minLength: 2, maxLength: 50 },
      { name: 'categoryType', label: 'Тип категория', type: 'select', required: true, options: categoryTypeOptions },
      { name: 'description', label: 'Описание', type: 'textarea', maxLength: 255 },
      { name: 'colorHex', label: 'Цвят на категорията', type: 'colorText', pattern: '^#[0-9A-Fa-f]{6}$', maxLength: 7 },
      { name: 'iconName', label: 'Икона', type: 'select', options: iconOptions, maxLength: 50 }
    ],
    sortOptions: [
      { value: 'name', label: 'Име' },
      { value: 'categoryType', label: 'Тип' },
      { value: 'createdAt', label: 'Дата' }
    ]
  },
  {
    key: 'financial-records',
    path: '/financial-records',
    title: 'Финансови записи',
    description: 'Тук се въвеждат реални приходи, разходи и платени абонаментни такси. Типът на записа се определя автоматично от избраната категория.',
    endpoint: '/financial-records',
    defaultSort: 'recordDate',
    defaultDirection: 'desc',
    needsCategories: true,
    categoryTypes: ['income', 'expense', 'subscription'],
    columns: [
      { key: 'recordDate', label: 'Дата', type: 'date' },
      { key: 'categoryName', label: 'Категория' },
      { key: 'recordType', label: 'Тип', options: recordTypeOptions },
      { key: 'amount', label: 'Сума', type: 'money' },
      { key: 'currency', label: 'Валута' }
    ],
    filters: [
      { name: 'searchQuery', label: 'Търсене', type: 'search' },
      { name: 'categoryId', label: 'Категория', type: 'category' },
      { name: 'recordType', label: 'Тип', type: 'select', options: recordTypeOptions },
      { name: 'fromDate', label: 'От дата', type: 'date' },
      { name: 'toDate', label: 'До дата', type: 'date' }
    ],
    fields: [
      { name: 'categoryId', label: 'Категория', type: 'category', required: true },
      { name: 'amount', label: 'Сума', type: 'number', required: true, min: 0.01, max: 100000000, step: '0.01' },
      { name: 'currency', label: 'Валута', type: 'select', required: true, options: currencyOptions, defaultValue: 'BGN' },
      { name: 'recordDate', label: 'Дата', type: 'date', required: true },
      { name: 'description', label: 'Описание', type: 'text', maxLength: 255 },
      { name: 'note', label: 'Бележка', type: 'textarea', maxLength: 1000 }
    ],
    sortOptions: [
      { value: 'recordDate', label: 'Дата' },
      { value: 'amount', label: 'Сума' },
      { value: 'categoryName', label: 'Категория' },
      { value: 'createdAt', label: 'Създаване' }
    ]
  },
  {
    key: 'subscriptions',
    path: '/subscriptions',
    title: 'Абонаменти',
    description: 'Тук се описват регулярни разходи като Spotify, Netflix, интернет или фитнес. Бутонът за такса записва плащането и премества следващата дата напред.',
    endpoint: '/subscriptions',
    defaultSort: 'nextDueDate',
    defaultDirection: 'asc',
    needsCategories: true,
    categoryTypes: ['subscription'],
    columns: [
      { key: 'name', label: 'Име' },
      { key: 'categoryName', label: 'Категория' },
      { key: 'amount', label: 'Сума', type: 'money' },
      { key: 'cycle', label: 'Цикъл', options: renewalCycleOptions },
      { key: 'nextDueDate', label: 'Следваща такса', type: 'date' },
      { key: 'status', label: 'Статус', options: subscriptionStatusOptions }
    ],
    filters: [
      { name: 'searchQuery', label: 'Търсене', type: 'search' },
      { name: 'categoryId', label: 'Категория', type: 'category' },
      { name: 'status', label: 'Статус', type: 'select', options: subscriptionStatusOptions },
      { name: 'cycle', label: 'Цикъл', type: 'select', options: renewalCycleOptions },
      { name: 'nextDueFrom', label: 'Такса от', type: 'date' },
      { name: 'nextDueTo', label: 'Такса до', type: 'date' }
    ],
    fields: [
      { name: 'categoryId', label: 'Абонаментна категория', type: 'category', required: true },
      { name: 'name', label: 'Име на абонамента', type: 'text', required: true, minLength: 2, maxLength: 100 },
      { name: 'amount', label: 'Сума на таксата', type: 'number', required: true, min: 0.01, max: 100000000, step: '0.01' },
      { name: 'currency', label: 'Валута', type: 'select', required: true, options: currencyOptions, defaultValue: 'BGN' },
      { name: 'cycle', label: 'Колко често се плаща', type: 'select', required: true, options: renewalCycleOptions, defaultValue: 'monthly' },
      { name: 'customCycleDays', label: 'Период в дни', type: 'number', required: true, min: 1, max: 365, showWhen: { name: 'cycle', value: 'custom' }, helper: 'Попълва се само ако цикълът е „По избор“. Това не е продължителност на абонамента, а през колко дни се повтаря таксата.' },
      { name: 'startDate', label: 'Начална дата', type: 'date', required: true, defaultValue: 'currentDate', helper: 'Следващата такса се изчислява автоматично според тази дата и цикъла.' },
      { name: 'status', label: 'Статус', type: 'select', required: true, options: subscriptionStatusOptions, defaultValue: 'active' },
      { name: 'description', label: 'Описание', type: 'textarea', maxLength: 255 }
    ],
    sortOptions: [
      { value: 'nextDueDate', label: 'Следваща такса' },
      { value: 'name', label: 'Име' },
      { value: 'amount', label: 'Сума' },
      { value: 'status', label: 'Статус' },
      { value: 'createdAt', label: 'Създаване' }
    ]
  },
  {
    key: 'budgets',
    path: '/budgets',
    title: 'Бюджети',
    description: 'Бюджетът е месечен лимит за разходна категория. Така системата може да сравни лимита с реално въведените разходи за същия месец.',
    endpoint: '/budgets',
    defaultSort: 'year',
    defaultDirection: 'desc',
    needsCategories: true,
    categoryTypes: ['expense'],
    columns: [
      { key: 'year', label: 'Година' },
      { key: 'month', label: 'Месец' },
      { key: 'categoryName', label: 'Разходна категория' },
      { key: 'limitAmount', label: 'Лимит', type: 'money' },
      { key: 'spentAmount', label: 'Похарчено', type: 'money' },
      { key: 'remainingAmount', label: 'Остава', type: 'budgetRemaining' }
    ],
    filters: [
      { name: 'month', label: 'Месец', type: 'number', min: 1, max: 12 },
      { name: 'year', label: 'Година', type: 'number', min: 2000, max: 2100 },
      { name: 'categoryId', label: 'Разходна категория', type: 'category' }
    ],
    fields: [
      { name: 'categoryId', label: 'Разходна категория', type: 'category', required: true },
      { name: 'month', label: 'Месец', type: 'number', required: true, min: 1, max: 12, defaultValue: 'currentMonth', helper: 'Избери месеца, за който важи лимитът.' },
      { name: 'year', label: 'Година', type: 'number', required: true, min: 2000, max: 2100, defaultValue: 'currentYear', helper: 'Избери годината на този месечен бюджет.' },
      { name: 'limitAmount', label: 'Месечен лимит', type: 'number', required: true, min: 0.01, max: 100000000, step: '0.01' },
      { name: 'currency', label: 'Валута', type: 'select', required: true, options: currencyOptions, defaultValue: 'BGN' },
      { name: 'description', label: 'Описание', type: 'textarea', maxLength: 255 }
    ],
    sortOptions: [
      { value: 'year', label: 'Година' },
      { value: 'month', label: 'Месец' },
      { value: 'limitAmount', label: 'Лимит' },
      { value: 'categoryName', label: 'Категория' },
      { value: 'createdAt', label: 'Създаване' }
    ]
  },
  {
    key: 'savings-goals',
    path: '/savings-goals',
    title: 'Спестовни цели',
    endpoint: '/savings-goals',
    defaultSort: 'targetDate',
    defaultDirection: 'asc',
    columns: [
      { key: 'name', label: 'Име' },
      { key: 'targetAmount', label: 'Цел', type: 'money' },
      { key: 'currentAmount', label: 'Текущо', type: 'money' },
      { key: 'progressPercent', label: 'Прогрес', type: 'percent' },
      { key: 'targetDate', label: 'Срок', type: 'date' },
      { key: 'status', label: 'Статус', options: savingsGoalStatusOptions }
    ],
    filters: [
      { name: 'searchQuery', label: 'Търсене', type: 'search' },
      { name: 'status', label: 'Статус', type: 'select', options: savingsGoalStatusOptions },
      { name: 'targetDateFrom', label: 'Срок от', type: 'date' },
      { name: 'targetDateTo', label: 'Срок до', type: 'date' }
    ],
    fields: [
      { name: 'name', label: 'Име', type: 'text', required: true, minLength: 2, maxLength: 100 },
      { name: 'targetAmount', label: 'Целева сума', type: 'number', required: true, min: 0.01, max: 1000000000, step: '0.01' },
      { name: 'currentAmount', label: 'Текуща сума', type: 'number', required: true, min: 0, max: 1000000000, step: '0.01', defaultValue: 0 },
      { name: 'currency', label: 'Валута', type: 'select', required: true, options: currencyOptions, defaultValue: 'BGN' },
      { name: 'targetDate', label: 'Срок', type: 'date', required: true },
      { name: 'status', label: 'Статус', type: 'select', required: true, options: savingsGoalStatusOptions, defaultValue: 'active' },
      { name: 'description', label: 'Описание', type: 'textarea', maxLength: 255 },
      { name: 'priority', label: 'Приоритет', type: 'number', min: 1, max: 5, defaultValue: 3 }
    ],
    sortOptions: [
      { value: 'targetDate', label: 'Срок' },
      { value: 'name', label: 'Име' },
      { value: 'targetAmount', label: 'Целева сума' },
      { value: 'currentAmount', label: 'Текуща сума' },
      { value: 'status', label: 'Статус' },
      { value: 'createdAt', label: 'Създаване' }
    ]
  }
];
