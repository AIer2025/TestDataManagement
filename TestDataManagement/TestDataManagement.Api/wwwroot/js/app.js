// ==========================================
// API 配置 - 使用相对路径以适应不同端口
// ==========================================
const API_BASE_URL = '';  // 使用相对路径，自动适应当前host

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

    // 绑定删失类型切换事件
    const censoringTypeSelect = document.getElementById('censoringType');
    if (censoringTypeSelect) {
        censoringTypeSelect.addEventListener('change', handleCensoringTypeChange);
        // 触发一次以设置初始状态
        handleCensoringTypeChange();
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
// 保存测试数据
// ==========================================
async function saveTestData() {
    try {
        showLoading(true);

        const testId = document.getElementById('testId').value;
        const isEdit = testId && testId.length > 0;

        // 收集表单数据
        const data = {
            moduleId: parseInt(document.getElementById('moduleId').value),
            testTime: document.getElementById('testTime').value,
            testValue: parseFloat(document.getElementById('testValue').value),  // 重要：添加testValue字段
            testUnit: document.getElementById('testUnit').value || 'hours',
            testType: document.getElementById('testType').value,
            testCycle: document.getElementById('testCycle').value ? parseInt(document.getElementById('testCycle').value) : null,
            quantity: parseInt(document.getElementById('quantity').value) || 1,
            censoringType: parseInt(document.getElementById('censoringType').value),
            failureMode: document.getElementById('failureMode').value || null,
            subsetId: document.getElementById('subsetId').value || null,
            temperature: document.getElementById('temperature').value ? parseFloat(document.getElementById('temperature').value) : null,
            humidity: document.getElementById('humidity').value ? parseFloat(document.getElementById('humidity').value) : null,
            operator: document.getElementById('operator').value || null,
            remarks: document.getElementById('remarks').value || null
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
    
    // 重置删失类型显示
    handleCensoringTypeChange();
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

            // 填充表单
            document.getElementById('testId').value = data.testId;
            document.getElementById('moduleId').value = data.moduleId;
            document.getElementById('testTime').value = formatDateTimeForInput(data.testTime);
            document.getElementById('testValue').value = data.testValue;
            document.getElementById('testUnit').value = data.testUnit || 'hours';
            document.getElementById('testType').value = data.testType;
            document.getElementById('testCycle').value = data.testCycle || '';
            document.getElementById('quantity').value = data.quantity;
            document.getElementById('censoringType').value = data.censoringType;
            document.getElementById('failureTime').value = data.failureTime || '';
            document.getElementById('lastInspectionTime').value = data.lastInspectionTime || 0;
            document.getElementById('failureMode').value = data.failureMode || '';
            document.getElementById('subsetId').value = data.subsetId || '';
            document.getElementById('temperature').value = data.temperature || '';
            document.getElementById('humidity').value = data.humidity || '';
            document.getElementById('operator').value = data.operator || '';
            document.getElementById('remarks').value = data.remarks || '';

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
