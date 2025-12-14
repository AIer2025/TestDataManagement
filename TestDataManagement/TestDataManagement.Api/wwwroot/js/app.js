// ==========================================
// API 配置 - 使用相对路径以适应不同端口
// ==========================================
const API_BASE_URL = '';  // 使用相对路径，自动适应当前host

// ==========================================
// 测试类型配置 - 定义每种测试类型的默认值、单位和显示名称
// ==========================================
const TEST_TYPE_CONFIG = {
    'LIFE_TEST': {
        labelName: '额定寿命时间',
        description: '产品在额定条件下的预期运行时间',
        defaultValue: 1000,
        defaultUnit: 'hours',
        units: ['hours', 'cycles']
    },
    'STRESS_TEST': {
        labelName: '应力水平值',
        description: '施加的机械应力或电气应力强度',
        defaultValue: 100,
        defaultUnit: 'MPa',
        units: ['MPa', 'N', 'V', 'A', 'W']
    },
    'BURN_IN': {
        labelName: '老化温度/功率',
        description: '加速老化测试时的温度或功率设定值',
        defaultValue: 85,
        defaultUnit: '℃',
        units: ['℃', 'W', 'V']
    },
    'ENVIRONMENTAL': {
        labelName: '环境应力值',
        description: '温度、湿度或振动等环境条件参数',
        defaultValue: 25,
        defaultUnit: '℃',
        units: ['℃', '%', 'Hz', 'rpm']
    }
};

// ==========================================
// 分页配置和状态
// ==========================================
const PAGE_SIZE = 20;  // 每页显示条数
let allData = [];      // 存储所有查询结果
let currentPage = 1;   // 当前页码
let totalPages = 0;    // 总页数

// ==========================================
// 页面加载完成后初始化
// ==========================================
document.addEventListener('DOMContentLoaded', function() {
    console.log('📋 页面加载完成,开始初始化...');

    // 加载模组列表（同时加载到录入和查询下拉框）
    loadModules();

    // 加载操作员列表
    loadOperators();

    // 加载子集列表
    loadSubsets();

    // 绑定删失类型切换事件
    const censoringTypeSelect = document.getElementById('censoringType');
    if (censoringTypeSelect) {
        censoringTypeSelect.addEventListener('change', handleCensoringTypeChange);
        // 触发一次以设置初始状态
        handleCensoringTypeChange();
    }

    // 绑定测试类型切换事件
    const testTypeSelect = document.getElementById('testType');
    if (testTypeSelect) {
        testTypeSelect.addEventListener('change', handleTestTypeChange);
        // 触发一次以设置初始状态
        handleTestTypeChange();
    }

    // 绑定数据录入表单提交事件
    const testDataForm = document.getElementById('testDataForm');
    if (testDataForm) {
        testDataForm.addEventListener('submit', function(e) {
            e.preventDefault();  // 阻止默认表单提交
            saveTestData();
        });
    }

    // 绑定取消编辑按钮
    const cancelBtn = document.getElementById('cancelBtn');
    if (cancelBtn) {
        cancelBtn.addEventListener('click', cancelEdit);
    }

    // 绑定查询表单提交事件
    const queryForm = document.getElementById('queryForm');
    if (queryForm) {
        queryForm.addEventListener('submit', function(e) {
            e.preventDefault();  // 阻止默认表单提交
            queryTestData();
        });
    }

    // 设置默认测试时间为当前时间
    const testTimeInput = document.getElementById('testTime');
    if (testTimeInput) {
        const now = new Date();
        now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
        testTimeInput.value = now.toISOString().slice(0, 16);
    }

    // 初始加载数据列表
    queryTestData();
});

// ==========================================
// 加载模组列表
// ==========================================
async function loadModules() {
    try {
        console.log('🔄 正在加载模组列表...');

        const response = await fetch(`${API_BASE_URL}/api/module`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        console.log('📊 响应状态:', response.status, response.statusText);

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();
        console.log('✅ API 返回结果:', result);

        // 获取录入表单的模组下拉框
        const selectInput = document.getElementById('moduleId');
        // 获取查询表单的模组下拉框
        const selectQuery = document.getElementById('queryModuleId');

        // 清空并填充选项
        if (selectInput) {
            selectInput.innerHTML = '<option value="">请选择模组</option>';
        }
        if (selectQuery) {
            selectQuery.innerHTML = '<option value="">全部模组</option>';
        }

        // 检查返回数据格式
        if (result.success && result.data && Array.isArray(result.data)) {
            console.log(`📦 共获取到 ${result.data.length} 个模组`);

            result.data.forEach(module => {
                // 添加到录入表单
                if (selectInput) {
                    const option = document.createElement('option');
                    option.value = module.moduleId;
                    option.textContent = `${module.moduleCode} - ${module.moduleName}`;
                    selectInput.appendChild(option);
                }
                // 添加到查询表单
                if (selectQuery) {
                    const option = document.createElement('option');
                    option.value = module.moduleId;
                    option.textContent = `${module.moduleCode} - ${module.moduleName}`;
                    selectQuery.appendChild(option);
                }
            });

            console.log('✅ 模组列表加载成功');
        } else {
            console.warn('⚠️ 返回数据格式异常:', result);
            showMessage('模组数据格式错误,请检查后端返回格式', 'error');
        }
    } catch (error) {
        console.error('❌ 加载模组失败:', error);
        showMessage(`加载模组列表失败: ${error.message}`, 'error');
    }
}

// ==========================================
// 加载操作员列表
// ==========================================
async function loadOperators() {
    try {
        console.log('🔄 正在加载操作员列表...');

        const response = await fetch(`${API_BASE_URL}/api/testdata/operators`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();
        console.log('✅ 操作员列表返回结果:', result);

        const selectOperator = document.getElementById('idOperator');

        if (selectOperator && result.success && result.data && Array.isArray(result.data)) {
            selectOperator.innerHTML = '<option value="">请选择操作员</option>';

            result.data.forEach(op => {
                const option = document.createElement('option');
                option.value = op.idOperator;
                option.textContent = op.operatorName;
                selectOperator.appendChild(option);
            });

            // 设置默认选中第一个操作员（ID=1）
            if (result.data.length > 0) {
                selectOperator.value = '1';
            }

            console.log(`✅ 操作员列表加载成功，共 ${result.data.length} 个操作员`);
        }
    } catch (error) {
        console.error('❌ 加载操作员列表失败:', error);
    }
}

// ==========================================
// 加载子集列表
// ==========================================
async function loadSubsets() {
    try {
        console.log('🔄 正在加载子集列表...');

        const response = await fetch(`${API_BASE_URL}/api/testdata/subsets`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();
        console.log('✅ 子集列表返回结果:', result);

        const selectSubset = document.getElementById('subsetId');

        if (selectSubset && result.success && result.data && Array.isArray(result.data)) {
            selectSubset.innerHTML = '<option value="">请选择子集</option>';

            result.data.forEach(subset => {
                const option = document.createElement('option');
                option.value = subset.subsetId;
                option.textContent = subset.subsetName;
                selectSubset.appendChild(option);
            });

            // 设置默认选中第一个子集（ID=1）
            if (result.data.length > 0) {
                selectSubset.value = '1';
            }

            console.log(`✅ 子集列表加载成功，共 ${result.data.length} 个子集`);
        }
    } catch (error) {
        console.error('❌ 加载子集列表失败:', error);
    }
}

// ==========================================
// 删失类型切换处理
// ==========================================
function handleCensoringTypeChange() {
    const censoringType = parseInt(document.getElementById('censoringType').value);
    const lastInspectionGroup = document.getElementById('lastInspectionGroup');
    const failureTimeLabel = document.getElementById('failureTimeLabel');
    const failureTimeInput = document.getElementById('failureTime');
    const censoringHelp = document.getElementById('censoringHelp');
    const failureModeSelect = document.getElementById('failureMode');

    // 根据删失类型设置字段
    switch(censoringType) {
        case 0: // 完全数据
            failureTimeLabel.innerHTML = '失效时间(小时) <span class="required">*</span>';
            failureTimeInput.placeholder = '精确的失效时间(小时)';
            lastInspectionGroup.style.display = 'none';
            censoringHelp.textContent = '完全数据: 观察到精确失效时间，state_flag=F, is_censored=0';
            // 完全数据可以选择失效模式
            if (failureModeSelect) failureModeSelect.disabled = false;
            break;
        case 1: // 右删失
            failureTimeLabel.innerHTML = '截止时间(小时) <span class="required">*</span>';
            failureTimeInput.placeholder = '测试终止时间(小时)';
            lastInspectionGroup.style.display = 'none';
            censoringHelp.textContent = '右删失数据: 样本在测试结束时仍未失效，state_flag=S, is_censored=1';
            // 右删失通常没有失效模式
            if (failureModeSelect) {
                failureModeSelect.value = '';
                failureModeSelect.disabled = true;
            }
            break;
        case 2: // 区间删失
            failureTimeLabel.innerHTML = '失效时间上界(小时) <span class="required">*</span>';
            failureTimeInput.placeholder = '下次检测时间(小时)';
            lastInspectionGroup.style.display = 'block';
            censoringHelp.textContent = '区间删失数据: 失效发生在两次检测之间，需要同时提供上界和下界';
            // 区间删失可以选择失效模式
            if (failureModeSelect) failureModeSelect.disabled = false;
            break;
        case 3: // 左删失
            failureTimeLabel.innerHTML = '首次检测时间(小时) <span class="required">*</span>';
            failureTimeInput.placeholder = '首次检测发现失效的时间(小时)';
            lastInspectionGroup.style.display = 'none';
            censoringHelp.textContent = '左删失数据: 首次检测时样本已失效，state_flag=F, is_censored=0';
            // 左删失可以选择失效模式
            if (failureModeSelect) failureModeSelect.disabled = false;
            break;
    }
}

// ==========================================
// 处理测试类型变化 - 动态更新测试值字段
// ==========================================
function handleTestTypeChange(isEditMode = false) {
    const testType = document.getElementById('testType').value;
    const config = TEST_TYPE_CONFIG[testType];

    if (!config) return;

    // 更新标签名称和说明
    const testValueLabel = document.getElementById('testValueLabel');
    if (testValueLabel) {
        testValueLabel.innerHTML = `${config.labelName} <span class="required">*</span>`;
    }

    // 更新提示说明
    const testValueHint = document.getElementById('testValueHint');
    if (testValueHint) {
        testValueHint.textContent = config.description;
    }

    // 更新单位下拉菜单 - 高亮推荐单位
    const testUnitSelect = document.getElementById('testUnit');
    if (testUnitSelect) {
        // 遍历所有选项，标记推荐单位
        Array.from(testUnitSelect.options).forEach(option => {
            if (config.units.includes(option.value)) {
                option.style.fontWeight = 'bold';
                option.style.color = '#4f46e5';
            } else {
                option.style.fontWeight = 'normal';
                option.style.color = '#6b7280';
            }
        });

        // 如果不是编辑模式，设置默认单位
        if (!isEditMode) {
            testUnitSelect.value = config.defaultUnit;
        }
    }

    // 如果不是编辑模式，设置默认值
    if (!isEditMode) {
        const testValueInput = document.getElementById('testValue');
        if (testValueInput) {
            testValueInput.value = config.defaultValue;
        }
    }

    console.log(`📝 测试类型切换为: ${testType}, 标签: ${config.labelName}, 默认值: ${config.defaultValue} ${config.defaultUnit}`);
}

// ==========================================
// 保存测试数据
// ==========================================
async function saveTestData() {
    try {
        showLoading(true);

        const testId = document.getElementById('testId').value;
        const isEdit = testId && testId.length > 0;

        // 收集表单数据
        const idOperatorValue = document.getElementById('idOperator').value;
        const subsetIdValue = document.getElementById('subsetId').value;

        const data = {
            moduleId: parseInt(document.getElementById('moduleId').value),
            testTime: document.getElementById('testTime').value,
            testValue: parseFloat(document.getElementById('testValue').value),  // 重要：添加testValue字段
            testUnit: document.getElementById('testUnit').value || 'hours',
            testType: document.getElementById('testType').value,
            testCycle: parseInt(document.getElementById('testCycle').value) || 1,
            quantity: parseInt(document.getElementById('quantity').value) || 1,
            censoringType: parseInt(document.getElementById('censoringType').value),
            failureMode: document.getElementById('failureMode').value || null,
            subsetId: subsetIdValue ? parseInt(subsetIdValue) : 1,
            temperature: parseFloat(document.getElementById('temperature').value) || 20,
            humidity: parseFloat(document.getElementById('humidity').value) || 60,
            idOperator: idOperatorValue ? parseInt(idOperatorValue) : 1,
            remarks: document.getElementById('remarks').value || '请输入备注说明~~~!!!'
        };

        // 根据删失类型添加时间字段
        const censoringType = data.censoringType;
        data.failureTime = parseFloat(document.getElementById('failureTime').value);

        if (censoringType === 2) {
            // 区间删失需要前次检测时间
            data.lastInspectionTime = parseFloat(document.getElementById('lastInspectionTime').value);
        } else {
            data.lastInspectionTime = 0;
        }

        // 验证必填字段
        if (!data.moduleId) {
            showMessage('请选择模组', 'error');
            showLoading(false);
            return;
        }
        if (!data.testTime) {
            showMessage('请选择测试时间', 'error');
            showLoading(false);
            return;
        }
        if (isNaN(data.testValue)) {
            showMessage('请输入测试值', 'error');
            showLoading(false);
            return;
        }
        if (isNaN(data.failureTime) || data.failureTime <= 0) {
            showMessage('请输入有效的失效时间', 'error');
            showLoading(false);
            return;
        }
        if (censoringType === 2 && (isNaN(data.lastInspectionTime) || data.lastInspectionTime <= 0)) {
            showMessage('区间删失数据必须提供有效的前次检测时间', 'error');
            showLoading(false);
            return;
        }

        console.log('💾 保存数据:', data);

        let url = `${API_BASE_URL}/api/testdata`;
        let method = 'POST';

        if (isEdit) {
            data.testId = parseInt(testId);
            url = `${API_BASE_URL}/api/testdata/${testId}`;
            method = 'PUT';
        }

        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });

        const result = await response.json();
        console.log('📡 API响应:', result);

        if (result.success) {
            showMessage(isEdit ? '更新成功!' : '保存成功!', 'success');
            // 清空表单
            resetForm();
            // 刷新数据列表
            queryTestData();
        } else {
            showMessage('保存失败: ' + result.message, 'error');
        }
    } catch (error) {
        console.error('❌ 保存失败:', error);
        showMessage('保存失败: ' + error.message, 'error');
    } finally {
        showLoading(false);
    }
}

// ==========================================
// 重置表单
// ==========================================
function resetForm() {
    document.getElementById('testDataForm').reset();
    document.getElementById('testId').value = '';
    document.getElementById('cancelBtn').style.display = 'none';
    document.getElementById('submitText').textContent = '💾 保存数据';

    // 重新设置默认时间
    const testTimeInput = document.getElementById('testTime');
    if (testTimeInput) {
        const now = new Date();
        now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
        testTimeInput.value = now.toISOString().slice(0, 16);
    }

    // 设置默认值
    document.getElementById('temperature').value = 20;
    document.getElementById('humidity').value = 60;
    document.getElementById('testCycle').value = 1;
    document.getElementById('remarks').value = '请输入备注说明~~~!!!';

    // 设置下拉菜单默认选中第一个有效选项（ID=1）
    const idOperatorSelect = document.getElementById('idOperator');
    const subsetIdSelect = document.getElementById('subsetId');
    if (idOperatorSelect) idOperatorSelect.value = '1';
    if (subsetIdSelect) subsetIdSelect.value = '1';

    // 重置删失类型显示
    handleCensoringTypeChange();

    // 重置测试类型并设置默认测试值和单位
    handleTestTypeChange(false);
}

// ==========================================
// 取消编辑
// ==========================================
function cancelEdit() {
    resetForm();
    showMessage('已取消编辑', 'info');
}

// ==========================================
// 查询测试数据
// ==========================================
async function queryTestData() {
    try {
        showLoading(true);

        const queryModuleId = document.getElementById('queryModuleId')?.value || '';
        const queryTestType = document.getElementById('queryTestType')?.value || '';
        const queryCensoringType = document.getElementById('queryCensoringType')?.value || '';
        const startDate = document.getElementById('startDate')?.value || '';
        const endDate = document.getElementById('endDate')?.value || '';

        let url = `${API_BASE_URL}/api/testdata?pageIndex=1&pageSize=10000`;
        if (queryModuleId) url += `&moduleId=${queryModuleId}`;
        if (queryTestType) url += `&testType=${queryTestType}`;
        if (queryCensoringType !== '') url += `&censoringType=${queryCensoringType}`;
        if (startDate) url += `&startDate=${startDate}`;
        if (endDate) url += `&endDate=${endDate}`;

        console.log('🔍 查询 URL:', url);

        const response = await fetch(url);
        const result = await response.json();

        console.log('📡 查询结果:', result);

        if (result.success) {
            // 保存所有数据到全局变量
            allData = result.data || [];
            totalPages = Math.ceil(allData.length / PAGE_SIZE);
            currentPage = 1;  // 重置到第一页

            // 渲染当前页数据
            renderCurrentPage();
            // 渲染分页控件
            renderPagination();

            showMessage(`查询成功，共 ${allData.length} 条数据，${totalPages} 页`, 'success');
        } else {
            showMessage('查询失败: ' + result.message, 'error');
        }
    } catch (error) {
        console.error('❌ 查询失败:', error);
        showMessage('查询失败: ' + error.message, 'error');
    } finally {
        showLoading(false);
    }
}

// ==========================================
// 渲染测试数据列表 - 修复ID为dataTableBody
// ==========================================
function renderTestDataList(data) {
    const tbody = document.getElementById('dataTableBody');  // 修复：使用正确的ID
    if (!tbody) {
        console.error('❌ 找不到 dataTableBody 元素');
        return;
    }

    tbody.innerHTML = '';

    if (!data || data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="10" class="no-data">暂无数据</td></tr>';
        return;
    }

    data.forEach(item => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${item.testId}</td>
            <td>${item.moduleCode || ''} - ${item.moduleName || ''}</td>
            <td>${formatDateTime(item.testTime)}</td>
            <td>${item.testType}</td>
            <td><span class="badge badge-${getCensoringTypeBadge(item.censoringType)}">${getCensoringTypeName(item.censoringType)}</span></td>
            <td>${formatFailureTime(item)}</td>
            <td>${item.failureMode || '-'}</td>
            <td>${item.temperature != null ? item.temperature + '℃' : '-'}</td>
            <td>${item.humidity != null ? item.humidity + '%' : '-'}</td>
            <td class="action-buttons">
                <button class="btn btn-sm btn-info" onclick="editTestData(${item.testId})">编辑</button>
                <button class="btn btn-sm btn-danger" onclick="deleteTestData(${item.testId})">删除</button>
            </td>
        `;
        tbody.appendChild(row);
    });
}

// ==========================================
// 编辑测试数据
// ==========================================
async function editTestData(testId) {
    try {
        showLoading(true);

        const response = await fetch(`${API_BASE_URL}/api/testdata/${testId}`);
        const result = await response.json();

        if (result.success && result.data) {
            const data = result.data;

            console.log('📝 编辑数据:', data);
            console.log('📝 idOperator:', data.idOperator, '子集ID:', data.subsetId);
            console.log('📝 测试值:', data.testValue, '单位:', data.testUnit);

            // 填充表单
            document.getElementById('testId').value = data.testId;
            document.getElementById('moduleId').value = data.moduleId;
            document.getElementById('testTime').value = formatDateTimeForInput(data.testTime);
            document.getElementById('testType').value = data.testType;

            // 先设置测试类型，再更新标签（编辑模式不覆盖值）
            handleTestTypeChange(true);

            // 然后设置实际的测试值和单位（来自数据库）
            document.getElementById('testValue').value = data.testValue;
            document.getElementById('testUnit').value = data.testUnit || 'hours';

            document.getElementById('testCycle').value = data.testCycle || 1;
            document.getElementById('quantity').value = data.quantity;
            document.getElementById('censoringType').value = data.censoringType;
            document.getElementById('failureTime').value = data.failureTime || '';
            document.getElementById('lastInspectionTime').value = data.lastInspectionTime || 0;
            document.getElementById('failureMode').value = data.failureMode || '';
            document.getElementById('temperature').value = data.temperature || 20;
            document.getElementById('humidity').value = data.humidity || 60;
            document.getElementById('remarks').value = data.remarks || '请输入备注说明~~~!!!';

            // 设置下拉菜单值（转为字符串）
            const subsetIdSelect = document.getElementById('subsetId');
            const idOperatorSelect = document.getElementById('idOperator');

            if (subsetIdSelect) {
                subsetIdSelect.value = String(data.subsetId || 1);
                console.log('📝 设置子集ID:', subsetIdSelect.value);
            }

            if (idOperatorSelect) {
                idOperatorSelect.value = String(data.idOperator || 1);
                console.log('📝 设置操作员ID:', idOperatorSelect.value);
            }

            // 更新UI
            handleCensoringTypeChange();
            document.getElementById('cancelBtn').style.display = 'inline-block';
            document.getElementById('submitText').textContent = '💾 更新数据';

            // 滚动到表单
            document.getElementById('formSection').scrollIntoView({ behavior: 'smooth' });

            showMessage('已加载数据，可以进行编辑', 'info');
        } else {
            showMessage('加载数据失败: ' + (result.message || '未知错误'), 'error');
        }
    } catch (error) {
        console.error('❌ 加载数据失败:', error);
        showMessage('加载数据失败: ' + error.message, 'error');
    } finally {
        showLoading(false);
    }
}

// ==========================================
// 删除测试数据
// ==========================================
async function deleteTestData(testId) {
    if (!confirm('确定要删除这条数据吗？此操作不可恢复！')) {
        return;
    }

    try {
        showLoading(true);

        const response = await fetch(`${API_BASE_URL}/api/testdata/${testId}`, {
            method: 'DELETE'
        });

        const result = await response.json();

        if (result.success) {
            showMessage('删除成功', 'success');
            queryTestData();
        } else {
            showMessage('删除失败: ' + result.message, 'error');
        }
    } catch (error) {
        console.error('❌ 删除失败:', error);
        showMessage('删除失败: ' + error.message, 'error');
    } finally {
        showLoading(false);
    }
}

// ==========================================
// 工具函数
// ==========================================
function getCensoringTypeName(type) {
    const names = {
        0: '完全数据',
        1: '右删失',
        2: '区间删失',
        3: '左删失'
    };
    return names[type] || '未知';
}

function getCensoringTypeBadge(type) {
    const badges = {
        0: 'success',
        1: 'warning',
        2: 'info',
        3: 'primary'
    };
    return badges[type] || 'secondary';
}

function formatDateTime(dateStr) {
    if (!dateStr) return '-';
    const date = new Date(dateStr);
    return date.toLocaleString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function formatDateTimeForInput(dateStr) {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    date.setMinutes(date.getMinutes() - date.getTimezoneOffset());
    return date.toISOString().slice(0, 16);
}

function formatFailureTime(item) {
    if (item.censoringType === 2) {
        // 区间删失显示区间
        return `(${item.lastInspectionTime || 0}, ${item.failureTime || 0}]`;
    }
    return item.failureTime != null ? item.failureTime.toString() : '-';
}

// ==========================================
// UI 辅助函数
// ==========================================
function showLoading(show) {
    const loading = document.getElementById('loading');
    if (loading) {
        loading.style.display = show ? 'flex' : 'none';
    }
}

function showMessage(text, type = 'info') {
    const messageDiv = document.getElementById('message');
    if (!messageDiv) return;

    messageDiv.textContent = text;
    messageDiv.className = `message message-${type}`;
    messageDiv.style.display = 'block';

    // 3秒后自动隐藏
    setTimeout(() => {
        messageDiv.style.display = 'none';
    }, 3000);
}

// ==========================================
// 分页功能
// ==========================================

// 渲染当前页数据
function renderCurrentPage() {
    const startIndex = (currentPage - 1) * PAGE_SIZE;
    const endIndex = startIndex + PAGE_SIZE;
    const pageData = allData.slice(startIndex, endIndex);
    renderTestDataList(pageData);
}

// 渲染分页控件
function renderPagination() {
    const paginationDiv = document.getElementById('pagination');
    if (!paginationDiv) return;

    if (totalPages <= 1) {
        paginationDiv.innerHTML = '';
        return;
    }

    paginationDiv.innerHTML = `
        <div class="pagination-container">
            <div class="pagination-info">
                第 <span class="current-page">${currentPage}</span> / <span class="total-pages">${totalPages}</span> 页，
                共 <span class="total-count">${allData.length}</span> 条数据
            </div>
            <div class="pagination-buttons">
                <button class="btn btn-page" onclick="goToPage(1)" ${currentPage === 1 ? 'disabled' : ''} title="首页">
                    ⏮ 首页
                </button>
                <button class="btn btn-page" onclick="prevPages(10)" ${currentPage <= 10 ? 'disabled' : ''} title="向前10页">
                    ⏪ 前10页
                </button>
                <button class="btn btn-page" onclick="prevPage()" ${currentPage === 1 ? 'disabled' : ''} title="上一页">
                    ◀ 上1页
                </button>
                <span class="page-input-group">
                    <input type="number" id="pageInput" class="page-input" min="1" max="${totalPages}" value="${currentPage}" 
                           onkeypress="if(event.key==='Enter') goToInputPage()">
                    <button class="btn btn-page btn-go" onclick="goToInputPage()">跳转</button>
                </span>
                <button class="btn btn-page" onclick="nextPage()" ${currentPage === totalPages ? 'disabled' : ''} title="下一页">
                    下1页 ▶
                </button>
                <button class="btn btn-page" onclick="nextPages(10)" ${currentPage > totalPages - 10 ? 'disabled' : ''} title="向后10页">
                    后10页 ⏩
                </button>
                <button class="btn btn-page" onclick="goToPage(${totalPages})" ${currentPage === totalPages ? 'disabled' : ''} title="末页">
                    末页 ⏭
                </button>
            </div>
        </div>
    `;
}

// 上一页
function prevPage() {
    if (currentPage > 1) {
        currentPage--;
        renderCurrentPage();
        renderPagination();
        scrollToTable();
    }
}

// 下一页
function nextPage() {
    if (currentPage < totalPages) {
        currentPage++;
        renderCurrentPage();
        renderPagination();
        scrollToTable();
    }
}

// 向前翻N页
function prevPages(n) {
    currentPage = Math.max(1, currentPage - n);
    renderCurrentPage();
    renderPagination();
    scrollToTable();
}

// 向后翻N页
function nextPages(n) {
    currentPage = Math.min(totalPages, currentPage + n);
    renderCurrentPage();
    renderPagination();
    scrollToTable();
}

// 跳转到指定页
function goToPage(page) {
    if (page >= 1 && page <= totalPages) {
        currentPage = page;
        renderCurrentPage();
        renderPagination();
        scrollToTable();
    }
}

// 从输入框跳转
function goToInputPage() {
    const input = document.getElementById('pageInput');
    if (input) {
        const page = parseInt(input.value);
        if (!isNaN(page) && page >= 1 && page <= totalPages) {
            goToPage(page);
        } else {
            showMessage(`请输入1到${totalPages}之间的页码`, 'warning');
            input.value = currentPage;
        }
    }
}

// 滚动到表格位置
function scrollToTable() {
    const dataSection = document.getElementById('dataSection');
    if (dataSection) {
        dataSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
}

// ==========================================
// Weibull 分析页面功能
// ==========================================

// 模组数据缓存
let moduleListData = [];
let selectedModuleId = null;

// 显示Weibull分析页面
function showWeibullAnalysisPage() {
    console.log('🔄 切换到 Weibull 分析页面...');

    // 更新页面标题
    document.title = 'Weibull 失效数据分析系统';
    const headerTitle = document.querySelector('header h1');
    if (headerTitle) {
        headerTitle.textContent = '📈 Weibull 失效数据分析系统';
    }

    // 隐藏数据录入页面的main
    const dataEntryMain = document.querySelector('main:not(#weibullAnalysisPage)');
    if (dataEntryMain) {
        dataEntryMain.style.display = 'none';
    }

    // 显示Weibull分析页面
    const weibullPage = document.getElementById('weibullAnalysisPage');
    if (weibullPage) {
        weibullPage.style.display = 'block';
    }

    // 隐藏分析结果区域
    const resultSection = document.getElementById('analysisResultSection');
    if (resultSection) {
        resultSection.style.display = 'none';
    }

    // 加载模组列表
    loadModuleListForAnalysis();
}

// 返回数据录入页面
function returnToDataEntry() {
    console.log('🔄 返回数据录入页面...');

    // 恢复页面标题
    document.title = 'Weibull 失效数据录入系统';
    const headerTitle = document.querySelector('header h1');
    if (headerTitle) {
        headerTitle.textContent = '📊 Weibull 失效数据录入系统';
    }

    // 显示数据录入页面的main
    const dataEntryMain = document.querySelector('main:not(#weibullAnalysisPage)');
    if (dataEntryMain) {
        dataEntryMain.style.display = 'block';
    }

    // 隐藏Weibull分析页面
    const weibullPage = document.getElementById('weibullAnalysisPage');
    if (weibullPage) {
        weibullPage.style.display = 'none';
    }

    // 重置选中状态
    selectedModuleId = null;
}

// 加载模组列表用于分析
async function loadModuleListForAnalysis() {
    try {
        console.log('🔄 正在加载模组列表用于分析...');

        const response = await fetch(`${API_BASE_URL}/api/module`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();

        if (result.success && result.data && Array.isArray(result.data)) {
            moduleListData = result.data;
            renderModuleTable(result.data);
            console.log(`✅ 加载了 ${result.data.length} 个模组`);
        } else {
            showMessage('加载模组列表失败', 'error');
        }
    } catch (error) {
        console.error('❌ 加载模组列表失败:', error);
        showMessage(`加载模组列表失败: ${error.message}`, 'error');
    }
}

// 渲染模组表格
function renderModuleTable(modules) {
    const tbody = document.getElementById('moduleTableBody');
    if (!tbody) return;

    if (modules.length === 0) {
        tbody.innerHTML = '<tr><td colspan="4" class="no-data">暂无模组数据</td></tr>';
        return;
    }

    tbody.innerHTML = modules.map(module => `
        <tr onclick="selectModule(${module.moduleId})" class="${selectedModuleId === module.moduleId ? 'selected' : ''}">
            <td>
                <input type="radio" name="moduleSelect" value="${module.moduleId}" 
                       ${selectedModuleId === module.moduleId ? 'checked' : ''}
                       onclick="event.stopPropagation(); selectModule(${module.moduleId})">
            </td>
            <td>${module.moduleId}</td>
            <td>${module.moduleCode}</td>
            <td>${module.moduleName}</td>
        </tr>
    `).join('');
}

// 选择模组
function selectModule(moduleId) {
    selectedModuleId = moduleId;
    console.log(`📌 选中模组 ID: ${moduleId}`);

    // 更新表格行样式
    const rows = document.querySelectorAll('#moduleTableBody tr');
    rows.forEach(row => {
        const radio = row.querySelector('input[type="radio"]');
        if (radio && parseInt(radio.value) === moduleId) {
            row.classList.add('selected');
            radio.checked = true;
        } else {
            row.classList.remove('selected');
        }
    });
}

// 分析选定模组
async function analyzeSelectedModule() {
    if (!selectedModuleId) {
        showMessage('请先选择一个模组', 'warning');
        return;
    }

    const selectedModule = moduleListData.find(m => m.moduleId === selectedModuleId);
    if (!selectedModule) {
        showMessage('未找到选中的模组', 'error');
        return;
    }

    console.log(`🔬 开始分析模组: ${selectedModule.moduleCode} (${selectedModule.moduleName})`);
    showLoading(true);

    try {
        const response = await fetch(`${API_BASE_URL}/api/weibullanalysis/module/${selectedModuleId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();

        if (result.success) {
            displayAnalysisResult(result.data, false);
            showMessage('模组分析完成', 'success');
        } else {
            showMessage(`分析失败: ${result.message}`, 'error');
        }
    } catch (error) {
        console.error('❌ 模组分析失败:', error);
        showMessage(`分析失败: ${error.message}`, 'error');
    } finally {
        showLoading(false);
    }
}

// 分析所有模组
async function analyzeAllModules() {
    console.log('📊 开始分析所有模组...');
    showLoading(true);

    try {
        const response = await fetch(`${API_BASE_URL}/api/weibullanalysis/all`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();

        if (result.success) {
            displayAnalysisResult(result.data, true);
            showMessage('所有模组分析完成', 'success');
        } else {
            showMessage(`分析失败: ${result.message}`, 'error');
        }
    } catch (error) {
        console.error('❌ 所有模组分析失败:', error);
        showMessage(`分析失败: ${error.message}`, 'error');
    } finally {
        showLoading(false);
    }
}

// 显示分析结果
function displayAnalysisResult(data, isAllModules) {
    const resultSection = document.getElementById('analysisResultSection');
    const chartContainer = document.getElementById('weibullChartContainer');
    const chartImage = document.getElementById('weibullChartImage');
    const chartLoading = document.getElementById('chartLoading');
    const reportContainer = document.getElementById('analysisReportContainer');
    const downloadButtons = document.getElementById('downloadButtons');

    // 显示结果区域
    resultSection.style.display = 'block';

    // 处理图形
    if (data.chartPath) {
        chartImage.src = `${API_BASE_URL}/${data.chartPath}?t=${Date.now()}`;
        chartImage.style.display = 'block';
        chartLoading.style.display = 'none';

        // 设置下载链接
        const downloadChartBtn = document.getElementById('downloadChartBtn');
        downloadChartBtn.href = `${API_BASE_URL}/${data.chartPath}`;
        downloadChartBtn.download = data.chartFileName || 'Weibull_Analysis_Chart.png';
    } else {
        chartImage.style.display = 'none';
        chartLoading.textContent = '暂无图形数据';
    }

    // 处理报告数据
    if (data.results && data.results.length > 0) {
        reportContainer.style.display = 'block';
        renderReportTable(data.results);

        // 设置Excel下载链接
        if (data.reportPath) {
            const downloadReportBtn = document.getElementById('downloadReportBtn');
            downloadReportBtn.href = `${API_BASE_URL}/${data.reportPath}`;
            downloadReportBtn.download = data.reportFileName || 'Weibull_Analysis_Report.xlsx';
            downloadButtons.style.display = 'flex';
        }
    } else {
        reportContainer.style.display = 'none';
    }

    // 滚动到结果区域
    resultSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

// 渲染分析报告表格
function renderReportTable(results) {
    const tbody = document.getElementById('reportTableBody');
    if (!tbody) return;

    tbody.innerHTML = results.map(r => `
        <tr>
            <td>${r.moduleId || r.moduleID || '-'}</td>
            <td>${r.moduleCode || '-'}</td>
            <td>${r.moduleName || '-'}</td>
            <td>${formatNumber(r.beta, 4)}</td>
            <td>${formatNumber(r.lowerBeta, 4)}</td>
            <td>${formatNumber(r.upperBeta, 4)}</td>
            <td>${formatNumber(r.eta, 2)}</td>
            <td>${formatNumber(r.lowerEta, 2)}</td>
            <td>${formatNumber(r.upperEta, 2)}</td>
            <td>${formatNumber(r.r2, 4)}</td>
            <td>${formatNumber(r.mttf, 2)}</td>
            <td>${formatNumber(r.b10, 2)}</td>
            <td>${formatNumber(r.b50, 2)}</td>
            <td>${formatNumber(r.b90, 2)}</td>
            <td>${r.totalN || '-'}</td>
            <td>${r.completeN || '-'}</td>
            <td>${r.rightCensN || '-'}</td>
            <td>${r.intervalCensN || '-'}</td>
            <td>${r.leftCensN || '-'}</td>
        </tr>
    `).join('');
}

// 格式化数字
function formatNumber(value, decimals) {
    if (value === null || value === undefined || isNaN(value)) {
        return '-';
    }
    return parseFloat(value).toFixed(decimals);
}

