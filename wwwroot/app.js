const statusEl = document.getElementById("status");
const medicineTableBody = document.querySelector("#medicineTable tbody");
const salesTableBody = document.querySelector("#salesTable tbody");
const medicineForm = document.getElementById("medicineForm");
const saleForm = document.getElementById("saleForm");
const saleMedicineSelect = document.getElementById("saleMedicineId");
const searchInput = document.getElementById("searchInput");
const searchButton = document.getElementById("searchButton");
const clearSearchButton = document.getElementById("clearSearchButton");

const state = {
  medicines: [],
  sales: [],
  searchText: ""
};

function setStatus(message, isError = false) {
  statusEl.textContent = message;
  statusEl.className = `status ${isError ? "error" : "ok"}`;
}

async function requestJson(url, options = {}) {
  const response = await fetch(url, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {})
    }
  });

  const data = await response.json().catch(() => ({}));
  if (!response.ok) {
    throw new Error(data.error || `Request failed (${response.status})`);
  }

  return data;
}

function daysUntil(dateValue) {
  const now = new Date();
  now.setHours(0, 0, 0, 0);
  const target = new Date(dateValue);
  target.setHours(0, 0, 0, 0);
  return Math.ceil((target - now) / (1000 * 60 * 60 * 24));
}

function renderMedicines() {
  medicineTableBody.innerHTML = "";

  if (state.medicines.length === 0) {
    const row = document.createElement("tr");
    row.innerHTML = `<td colspan="6">No medicines found.</td>`;
    medicineTableBody.appendChild(row);
    updateSaleMedicineOptions();
    return;
  }

  for (const medicine of state.medicines) {
    const row = document.createElement("tr");
    const expiryDays = daysUntil(medicine.expiryDate);

    if (expiryDays < 30) {
      row.classList.add("expiring-soon");
    } else if (medicine.quantity < 10) {
      row.classList.add("low-stock");
    }

    row.innerHTML = `
      <td>${medicine.fullName}</td>
      <td>${new Date(medicine.expiryDate).toLocaleDateString()}</td>
      <td>${medicine.quantity}</td>
      <td>${Number(medicine.price).toFixed(2)}</td>
      <td>${medicine.brand}</td>
      <td><button type="button" class="small">Sell 1</button></td>
    `;

    const sellButton = row.querySelector("button");
    sellButton.addEventListener("click", async () => {
      try {
        await requestJson("/api/sales", {
          method: "POST",
          body: JSON.stringify({
            medicineId: medicine.id,
            quantitySold: 1
          })
        });
        setStatus("Sale recorded.");
        await Promise.all([loadMedicines(), loadSales()]);
      } catch (error) {
        setStatus(error.message, true);
      }
    });

    medicineTableBody.appendChild(row);
  }

  updateSaleMedicineOptions();
}

function updateSaleMedicineOptions() {
  const previousSelection = saleMedicineSelect.value;
  saleMedicineSelect.innerHTML = "";

  if (state.medicines.length === 0) {
    const option = document.createElement("option");
    option.value = "";
    option.textContent = "No medicines available";
    saleMedicineSelect.appendChild(option);
    saleMedicineSelect.disabled = true;
    return;
  }

  saleMedicineSelect.disabled = false;
  for (const medicine of state.medicines) {
    const option = document.createElement("option");
    option.value = medicine.id;
    option.textContent = `${medicine.fullName} (${medicine.quantity} in stock)`;
    saleMedicineSelect.appendChild(option);
  }

  if (previousSelection && [...saleMedicineSelect.options].some(o => o.value === previousSelection)) {
    saleMedicineSelect.value = previousSelection;
  }
}

function renderSales() {
  salesTableBody.innerHTML = "";

  if (state.sales.length === 0) {
    const row = document.createElement("tr");
    row.innerHTML = `<td colspan="5">No sale records found.</td>`;
    salesTableBody.appendChild(row);
    return;
  }

  for (const sale of state.sales) {
    const total = Number(sale.unitPrice) * Number(sale.quantitySold);
    const row = document.createElement("tr");
    row.innerHTML = `
      <td>${new Date(sale.soldAt).toISOString().replace("T", " ").slice(0, 19)}</td>
      <td>${sale.medicineName}</td>
      <td>${sale.quantitySold}</td>
      <td>${Number(sale.unitPrice).toFixed(2)}</td>
      <td>${total.toFixed(2)}</td>
    `;
    salesTableBody.appendChild(row);
  }
}

async function loadMedicines() {
  const query = state.searchText ? `?search=${encodeURIComponent(state.searchText)}` : "";
  state.medicines = await requestJson(`/api/medicines${query}`);
  renderMedicines();
}

async function loadSales() {
  state.sales = await requestJson("/api/sales");
  renderSales();
}

medicineForm.addEventListener("submit", async event => {
  event.preventDefault();
  const form = new FormData(medicineForm);

  const payload = {
    fullName: form.get("fullName"),
    notes: form.get("notes"),
    expiryDate: form.get("expiryDate"),
    quantity: Number.parseInt(form.get("quantity"), 10),
    price: Number.parseFloat(form.get("price")),
    brand: form.get("brand")
  };

  try {
    await requestJson("/api/medicines", {
      method: "POST",
      body: JSON.stringify(payload)
    });
    medicineForm.reset();
    setStatus("Medicine added successfully.");
    await loadMedicines();
  } catch (error) {
    setStatus(error.message, true);
  }
});

saleForm.addEventListener("submit", async event => {
  event.preventDefault();
  const form = new FormData(saleForm);

  const payload = {
    medicineId: form.get("medicineId"),
    quantitySold: Number.parseInt(form.get("quantitySold"), 10)
  };

  try {
    await requestJson("/api/sales", {
      method: "POST",
      body: JSON.stringify(payload)
    });
    saleForm.reset();
    setStatus("Sale saved successfully.");
    await Promise.all([loadMedicines(), loadSales()]);
  } catch (error) {
    setStatus(error.message, true);
  }
});

searchButton.addEventListener("click", async () => {
  state.searchText = searchInput.value.trim();
  try {
    await loadMedicines();
  } catch (error) {
    setStatus(error.message, true);
  }
});

clearSearchButton.addEventListener("click", async () => {
  searchInput.value = "";
  state.searchText = "";
  try {
    await loadMedicines();
  } catch (error) {
    setStatus(error.message, true);
  }
});

searchInput.addEventListener("keydown", async event => {
  if (event.key !== "Enter") {
    return;
  }

  event.preventDefault();
  state.searchText = searchInput.value.trim();
  try {
    await loadMedicines();
  } catch (error) {
    setStatus(error.message, true);
  }
});

async function initialize() {
  try {
    await Promise.all([loadMedicines(), loadSales()]);
    setStatus("Ready.");
  } catch (error) {
    setStatus(error.message, true);
  }
}

initialize();
