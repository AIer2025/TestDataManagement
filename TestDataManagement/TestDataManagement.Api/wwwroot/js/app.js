// API 基础地址
const API_BASE_URL = 'http://localhost:5000/api';

// 全局变量
let currentPage = 1;
const pageSize = 20;
let isEditMode = false;

// 初始化
const init = async () => {
    await loadModules();
    await loadTestData();
    bindEvents();
    updateCensoringTypeHelp();
};

// 加载模组列表
const loadModules = async () => {
    try {
        const response = await fetch(`${API_BASE_URL}/Module`);
        const result = await response.json();

        if (result.success) {
            const modules = result.data;
            const moduleSelect = document.getElementById('moduleId');
            const queryModuleSelect = document.getElementById('queryModuleId');

            // 清空并填充选项
            moduleSelect.innerHTML = '<option value="">请选择模组</option>';
            queryModuleSelect.innerHTML = '<option value="">全部模组</option>';

            modules.forEach(module => {
                const option = `<option value="${module.moduleId}">${module.moduleCode} - ${module.moduleName}</option>`;
                moduleSelect.innerHTML += option;
                queryModuleSelect.innerHTML += option;
            });
        }
    } catch (error) {
        showMessage('加载模组列表失败', 'error');
        console.error(error);
    }
};

// 加载测试数据
const loadTestData = async (page = 1) => {
    showLoading(true);
    try {
        const query = getQueryParams();
        query.pageIndex = page;
        query.pageSize = pageSize;

        const queryString = new URLSearchParams(query).toString();
        const response = await fetch(`${API_BASE_URL}/TestData?${queryString}`);
        const result = await response.json();

        if (result.success) {
            renderDataTable(result.data);
            renderPagination(result.totalCount, page);
            currentPage = page;
        } else {
            showMessage(result.message, 'error');
        }
    } catch (error) {
        showMessage('加载数据失败', 'error');
        console.error(error);
    } finally {
        showLoading(false);
    }
};

// 获取查询参数
const getQueryParams = () => {
    const params = {};
    const moduleId = document.getElementById('queryModuleId').value;
    const testType = document.getElementById('queryTestType').value;
    const censoringType = document.getElementById('queryCensoringType').value;
    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;

    if (moduleId) params.moduleId = moduleId;
    if (testType) params.testType = testType;
    if (censoringType !== '') params.censoringType = censoringType;
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;

    return params;
};

// 渲染数据表格
const renderDataTable = (data) => {
    const tbody = document.getElementById('dataTableBody');

    if (!data || data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="10" class="no-data">暂无数据</td></tr>';
        return;
    }

    tbody.innerHTML = data.map(item => `
        <tr>
            <td>${item.testId}</td>
            <td>${item.moduleCode} - ${item.moduleName}</td>
            <td>${formatDateTime(item.testTime)}</td>
            <td>${item.testType}</td>
            <td><span class="censoring-badge censoring-${item.censoringType}">${getCensoringTypeName(item.censoringType)}</span></td>
            <td>${item.failureTime ? item.failureTime.toFixed(2) : '-'}</td>
            <td>${item.failureMode || '-'}</td>
            <td>${item.temperature ? item.temperature.toFixed(1) : '-'}</td>
            <td>${item.humidity ? item.humidity.toFixed(1) : '-'}</td>
            <td>
                <button class="btn btn-edit" onclick="editData(${item.testId})"><\/button>
                <button class="btn btn-danger" onclick="deleteData(${item.testId})"><\/button>
            </td>
        </tr>
    `).join('');
};

// 渲染分页
const renderPagination = (totalCount, currentPage) => {
    const totalPages = Math.ceil(totalCount / pageSize);
    const pagination = document.getElementById('pagination');

    if (totalPages <= 1) {
        pagination.innerHTML = '';
        return;
    }

    pagination.innerHTML = `
        <button ${currentPage === 1 ? 'disabled' : ''} onclick="loadTestData(${currentPage - 1})">上一页</button>
        <span class="page-info">第 ${currentPage} / ${totalPages} 页 (共 ${totalCount} 条)</span>
        <button ${currentPage === totalPages ? 'disabled' : ''} onclick="loadTestData(${currentPage + 1})">下一页</button>
    `;
};

// 绑定事件
const bindEvents = () => {
    // 表单提交
    document.getElementById('testDataForm').addEventListener('submit', handleFormSubmit);

    // 查询表单提交
    document.getElementById('queryForm').addEventListener('submit', (e) => {
        e.preventDefault();
        loadTestData(1);
    });

    // 删失类型变化
    document.getElementById('censoringType').addEventListener('change', updateCensoringTypeHelp);

    // 取消编辑
    document.getElementById('cancelBtn').addEventListener('click', resetForm);
};

// 处理表单提交
const handleFormSubmit = async (e) => {
    e.preventDefault();

    const formData = getFormData();

    // 验证表单
    if (!validateForm(formData)) {
        return;
    }

    showLoading(true);
    try {
        let response;
        if (isEditMode) {
            response = await fetch(`${API_BASE_URL}/TestData/${formData.testId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(formData)
            });
        } else {
            response = await fetch(`${API_BASE_URL}/TestData`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(formData)
            });
        }

        const result = await response.json();

        if (result.success) {
            showMessage(isEditMode ? '更新成功' : '数据录入成功', 'success');
            resetForm();
            await loadTestData(currentPage);
        } else {
            showMessage(result.message, 'error');
        }
    } catch (error) {
        showMessage('操作失败', 'error');
        console.error(error);
    } finally {
        showLoading(false);
    }
};

// 获取表单数据
const getFormData = () => {
    const censoringType = parseInt(document.getElementById('censoringType').value);
    const testId = document.getElementById('testId').value;

    const data = {
        moduleId: parseInt(document.getElementById('moduleId').value),
        testTime: document.getElementById('testTime').value,
        testValue: parseFloat(document.getElementById('testValue').value),
        testUnit: document.getElementById('testUnit').value || 'hours',
        testType: document.getElementById('testType').value,
        quantity: parseInt(document.getElementById('quantity').value),
        censoringType: censoringType,
        failureTime: parseFloat(document.getElementById('failureTime').value) || null,
        lastInspectionTime: censoringType === 2 ? parseFloat(document.getElementById('lastInspectionTime').value) || 0 : 0,
        failureMode: document.getElementById('failureMode').value || null,
        subsetId: document.getElementById('subsetId').value || null,
        temperature: parseFloat(document.getElementById('temperature').value) || null,
        humidity: parseFloat(document.getElementById('humidity').value) || null,
        operator: document.getElementById('operator').value || null,
        testCycle: parseInt(document.getElementById('testCycle').value) || null,
        remarks: document.getElementById('remarks').value || null
    };

    if (testId) {
        data.testId = parseInt(testId);
    }

    return data;
};

// 验证表单
const validateForm = (data) => {
    if (!data.moduleId) {
        showMessage('请选择模组', 'warning');
        return false;
    }

    if (!data.failureTime) {
        showMessage('请输入失效时间', 'warning');
        return false;
    }

    if (data.censoringType === 2) {
        if (!data.lastInspectionTime || data.lastInspectionTime <= 0) {
            showMessage('区间删失数据必须提供前次检测时间', 'warning');
            return false;
        }
        if (data.failureTime <= data.lastInspectionTime) {
            showMessage('失效时间必须大于前次检测时间', 'warning');
            return false;
        }
    }

    if (data.humidity !== null && (data.humidity < 0 || data.humidity > 100)) {
        showMessage('湿度必须在0-100之间', 'warning');
        return false;
    }

    return true;
};

// 编辑数据
const editData = async (testId) => {
    showLoading(true);
    try {
        const response = await fetch(`${API_BASE_URL}/TestData/${testId}`);
        const result = await response.json();

        if (result.success) {
            fillForm(result.data);
            isEditMode = true;
            document.getElementById('submitText').textContent = '💾 更新数据';
            document.getElementById('cancelBtn').style.display = 'inline-block';
            document.getElementById('formSection').scrollIntoView({ behavior: 'smooth' });
        } else {
            showMessage(result.message, 'error');
        }
    } catch (error) {
        showMessage('加载数据失败', 'error');
        console.error(error);
    } finally {
        showLoading(false);
    }
};

// 填充表单
const fillForm = (data) => {
    document.getElementById('testId').value = data.testId;
    document.getElementById('moduleId').value = data.moduleId;
    document.getElementById('testTime').value = formatDateTimeForInput(data.testTime);
    document.getElementById('testValue').value = data.testValue;
    document.getElementById('testUnit').value = data.testUnit || 'hours';
    document.getElementById('testType').value = data.testType;
    document.getElementById('quantity').value = data.quantity;
    document.getElementById('censoringType').value = data.censoringType;
    document.getElementById('failureTime').value = data.failureTime;
    document.getElementById('lastInspectionTime').value = data.lastInspectionTime || 0;
    document.getElementById('failureMode').value = data.failureMode || '';
    document.getElementById('subsetId').value = data.subsetId || '';
    document.getElementById('temperature').value = data.temperature || '';
    document.getElementById('humidity').value = data.humidity || '';
    document.getElementById('operator').value = data.operator || '';
    document.getElementById('testCycle').value = data.testCycle || '';
    document.getElementById('remarks').value = data.remarks || '';

    updateCensoringTypeHelp();
};

// 删除数据
const deleteData = async (testId) => {
    if (!confirm('确认删除该条数据吗？')) {
        return;
    }

    showLoading(true);
    try {
        const response = await fetch(`${API_BASE_URL}/TestData/${testId}`, {
            method: 'DELETE'
        });
        const result = await response.json();

        if (result.success) {
            showMessage('删除成功', 'success');
            await loadTestData(currentPage);
        } else {
            showMessage(result.message, 'error');
        }
    } catch (error) {
        showMessage('删除失败', 'error');
        console.error(error);
    } finally {
        showLoading(false);
    }
};

// 重置表单
const resetForm = () => {
    document.getElementById('testDataForm').reset();
    document.getElementById('testId').value = '';
    document.getElementById('testUnit').value = 'hours';
    document.getElementById('quantity').value = 1;
    document.getElementById('lastInspectionTime').value = 0;
    isEditMode = false;
    document.getElementById('submitText').textContent = '💾 保存数据';
    document.getElementById('cancelBtn').style.display = 'none';
    updateCensoringTypeHelp();
};

// 更新删失类型帮助信息
const updateCensoringTypeHelp = () => {
    const censoringType = parseInt(document.getElementById('censoringType').value);
    const helpText = document.getElementById('censoringHelp');
    const failureTimeLabel = document.getElementById('failureTimeLabel');
    const lastInspectionGroup = document.getElementById('lastInspectionGroup');
    const lastInspectionInput = document.getElementById('lastInspectionTime');

    const helpTexts = {
        0: '完全数据: 观察到精确失效时间',
        1: '右删失数据: 样本在测试结束时仍未失效(悬置)',
        2: '区间删失数据: 只知道失效发生在两次检测之间',
        3: '左删失数据: 首次检测时已经失效'
    };

    const labelTexts = {
        0: '失效时间(小时)',
        1: '截止时间(小时)',
        2: '失效时间上界(小时)',
        3: '首次检测时间(小时)'
    };

    helpText.textContent = helpTexts[censoringType];
    failureTimeLabel.innerHTML = `${labelTexts[censoringType]} <span class="required">*</span>`;

    // 显示/隐藏前次检测时间字段
    if (censoringType === 2) {
        lastInspectionGroup.style.display = 'block';
        lastInspectionInput.required = true;
    } else {
        lastInspectionGroup.style.display = 'none';
        lastInspectionInput.required = false;
        lastInspectionInput.value = 0;
    }
};

// 工具函数
const showLoading = (show) => {
    document.getElementById('loading').style.display = show ? 'flex' : 'none';
};

const showMessage = (message, type = 'success') => {
    const messageDiv = document.getElementById('message');
    messageDiv.textContent = message;
    messageDiv.className = `message ${type}`;
    messageDiv.style.display = 'block';

    setTimeout(() => {
        messageDiv.style.display = 'none';
    }, 3000);
};

const formatDateTime = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
};

const formatDateTimeForInput = (dateString) => {
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
};

const getCensoringTypeName = (type) => {
    const names = {
        0: '完全数据',
        1: '右删失',
        2: '区间删失',
        3: '左删失'
    };
    return names[type] || '未知';
};

// 页面加载完成后初始化
window.addEventListener('DOMContentLoaded', init);